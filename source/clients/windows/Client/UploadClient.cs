using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using WebTimer.Client.Models;

namespace WebTimer.Client
{
    public class UploadClient
    {
        static bool uploadFlag = false;
        static string credentials = null;
        static string deviceId = null;
        static string deviceName = null;
        static object configLock = new object();
        static bool started = false;
        static object startLock = new object();
        static bool recovering = false;
        static List<Record> sendQueue = new List<Record>();
        static ControlMessage collectionStatus = ControlMessage.Normal;

        public enum ControlMessage
        {
            Normal = 0,
            SuspendCollection = 1,
            DisableDevice = 2
        }

        /// <summary>
        /// Start the upload client
        /// </summary>
        public static void Start()
        {
            lock (startLock)
            {
                if (!started)
                {
                    TraceLog.TraceInfo("Starting uploader");

                    // retrieve creds and device ID
                    credentials = ConfigClient.Read(ConfigClient.Credentials);
                    deviceId = ConfigClient.Read(ConfigClient.DeviceId);
                    deviceName = ConfigClient.Read(ConfigClient.DeviceName);

                    // start the uploader loop on a new thread
                    ThreadStart ts = new ThreadStart(SendLoop);
                    Thread thread = new Thread(ts);
                    uploadFlag = true;
                    thread.Start();
                    started = true;
                }
            }
        }

        /// <summary>
        /// stop the upload client
        /// </summary>
        public static void Stop()
        {
            lock (startLock)
            {
                TraceLog.TraceInfo("Stopping uploader");

                // terminate the loop
                uploadFlag = false;

                // one last send
                TraceLog.TraceInfo("Stop processing: sending last buffer");
                Send();

                TraceLog.TraceInfo("Stopped uploader");
            }
        }

        #region Helpers

        private static void SendLoop()
        {
            while (uploadFlag)
            {
                try
                {
                    lock (configLock)
                    {
                        // refresh the device enabled flag
                        var disabled = ConfigClient.Read(ConfigClient.Disabled);
                        if (Convert.ToBoolean(disabled))
                            collectionStatus = ControlMessage.SuspendCollection;

                        // refresh the credentials, device ID, device name in case the config tool was run
                        credentials = ConfigClient.Read(ConfigClient.Credentials, true);
                        if (deviceId == null)
                            deviceId = ConfigClient.Read(ConfigClient.DeviceId);
                        if (deviceName == null)
                            deviceName = ConfigClient.Read(ConfigClient.DeviceName);
                        if (credentials != null)
                            Send();
                    }
                }
                catch (Exception ex)
                {
                    TraceLog.TraceException("GetVersionLoop failed", ex);
                }
                
                // sleep for a minute
                Thread.Sleep(60000);
            }
        }

        private static void Send()
        {
            try
            {
                // snap the record list
                var recordList = CollectorClient.GetRecords();
                
                // if no records, may need to recover capture service
                if (recordList.Count == 0)
                {
                    if (recovering)
                    {
                        // this is the second timeout period that we didn't see records.  
                        // attempt to restart the collector
                        CollectorClient.Start();
                        recovering = false;
                    }
                    else
                    {
                        // this will trigger recovery if the next run through also has zero records
                        recovering = true;
                    }

                    // nothing to do this time (no records to process)
                    return;
                }
                else
                {
                    // reset recovery mode
                    recovering = false;
                }
                
                // compact the record list
                var uniqueHosts = recordList.Select(r => r.HostMacAddress).Distinct();
                foreach (var host in uniqueHosts)
                {
                    var uniqueDests = recordList.Where(r => r.HostMacAddress == host).Select(r => r.WebsiteName).Distinct();
                    foreach (var dest in uniqueDests)
                    {
                        var records = recordList.Where(r => r.HostMacAddress == host && r.WebsiteName == dest).OrderBy(r => r.Timestamp);

                        // only keep the first and last record so that the duration calculated on the server will be > 0
                        var firstRecord = records.FirstOrDefault();
                        var lastRecord = records.LastOrDefault();
                        
                        // if the last record is a second or more later, add the duration to the first record
                        TimeSpan duration;
                        if (lastRecord != null && lastRecord != firstRecord &&
                            (duration = Convert.ToDateTime(lastRecord.Timestamp) - Convert.ToDateTime(firstRecord.Timestamp))
                                >= TimeSpan.FromSeconds(1.0))
                        {
                            firstRecord.Duration = (int)duration.TotalSeconds;
                        }

                        lock (sendQueue)
                        {
                            var existingRecord = sendQueue.FirstOrDefault(r => r.HostMacAddress == host && r.WebsiteName == dest);
                            if (existingRecord == null || 
                                Convert.ToDateTime(firstRecord.Timestamp) - (Convert.ToDateTime(existingRecord.Timestamp) + TimeSpan.FromSeconds(existingRecord.Duration)) >= TimeSpan.FromSeconds(300.0)) // more than 5 minutes between records
                            {
                                // add the record to the send queue
                                sendQueue.Add(firstRecord);
                            }
                            else
                            {
                                // augment existing record with new duration
                                duration = Convert.ToDateTime(firstRecord.Timestamp) - Convert.ToDateTime(existingRecord.Timestamp) + TimeSpan.FromSeconds(firstRecord.Duration);
                                existingRecord.Duration = (int)duration.TotalSeconds;
                            }
                        }
                    }
                }

                if (sendQueue.Count > 0)
                {
                    // snap the send queue
                    var recordBuffer = new List<Record>();
                    lock (sendQueue)
                    {
                        // clear this buffer if collection is suspended
                        if (collectionStatus == ControlMessage.SuspendCollection)
                        {
                            sendQueue.Clear();
                            TraceLog.TraceInfo("Probing for new collection status");
                        }
                        else
                        {
                            recordBuffer.AddRange(sendQueue);
                            TraceLog.TraceInfo(String.Format("Sending records: {0}", JsonConvert.SerializeObject(sendQueue)));
                        }
                    }

                    var sendList = new RecordList()
                    {
                        DeviceId = deviceId,
                        DeviceName = deviceName,
                        SoftwareVersion = UpdateClient.CurrentVersion.ToString(),
                        Records = recordBuffer
                    };

                    // send the queue to the web service
                    WebServiceHelper.PostRecords(
                        credentials, 
                        sendList, 
                        new WebServiceHelper.PostRecordsDelegate((response) =>
                        {
                            // a callback means the service processed the records successfully; we can free them now.
                            // note a slight race condition - if the callback takes longer than the sleep interval, we 
                            // could lose a buffer.  since the sleep interval is 60sec, this isn't an important issue
                            // to address.
                            lock (sendQueue)
                            {
                                sendQueue.Clear();
                            }
                            var msg = (ControlMessage)response.controlMessage;
                            if (collectionStatus == msg)
                                return;

                            // process the control message
                            lock (configLock)
                            {
                                switch (msg)
                                {
                                    case ControlMessage.Normal:
                                        TraceLog.TraceInfo("Changing collection status to Normal");
                                        collectionStatus = msg;
                                        ConfigClient.Write(ConfigClient.Disabled, Convert.ToString(false));
                                        break;
                                    case ControlMessage.SuspendCollection:
                                        TraceLog.TraceInfo("Changing collection status to Suspended");
                                        collectionStatus = msg;
                                        ConfigClient.Write(ConfigClient.Disabled, Convert.ToString(true));
                                        break;
                                    case ControlMessage.DisableDevice:
                                        TraceLog.TraceInfo("Disabling the device by wiping the config");
                                        collectionStatus = msg;
                                        // wipe the config
                                        ConfigClient.Clear();
                                        break;
                                    default:
                                        // new message we don't understand yet
                                        TraceLog.TraceInfo(string.Format("Received unknown control message {0}", msg));
                                        break;
                                }
                            }
                        }), 
                        new WebServiceHelper.NetOpDelegate((inProgress, status) =>
                        {
                            if (status == OperationStatus.Retry)
                            {
                                // no need to do anything: next iteration will send original credentials
                            }
                            // no failure state to clean up
                        }));
                }
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("Upload processing failed", ex);
            }
        }

        #endregion
    }
}
