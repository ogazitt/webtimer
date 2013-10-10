using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Collector
{
    public class UploadClient
    {
        static bool uploadFlag = false;
        static string credentials = null;
        static object credentialsLock = new object();

        /// <summary>
        /// Start the upload client
        /// </summary>
        public static void Start()
        {
            TraceLog.TraceInfo("Starting uploader");

            credentials = ConfigClient.Read(ConfigClient.Credentials);

            ThreadStart ts = new ThreadStart(SendLoop);
            Thread thread = new Thread(ts);
            uploadFlag = true;
            thread.Start();
        }

        /// <summary>
        /// stop the upload client
        /// </summary>
        public static void Stop()
        {
            TraceLog.TraceInfo("Stopping uploader");

            // terminate the loop
            uploadFlag = false;
            
            // one last send
            TraceLog.TraceInfo("Stop processing: sending last buffer");
            Send();

            TraceLog.TraceInfo("Stopped uploader");
        }

        #region Helpers

        private static void SendLoop()
        {
            while (uploadFlag)
            {
                try
                {
                    lock (credentialsLock)
                    {
                        // refresh the credentials in case the config tool was run
                        credentials = ConfigClient.Read(ConfigClient.Credentials, true);
                        if (credentials != null)
                            Send();
                    }
                }
                catch (Exception ex)
                {
                    TraceLog.TraceException("SendLoop failed", ex);
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

                var sendQueue = new List<Record>();

                // compact the record list
                var uniqueHosts = recordList.Select(r => r.HostName).Distinct();
                foreach (var host in uniqueHosts)
                {
                    var uniqueDests = recordList.Where(r => r.HostName == host).Select(r => r.WebsiteName).Distinct();
                    foreach (var dest in uniqueDests)
                    {
                        var records = recordList.Where(r => r.HostName == host && r.WebsiteName == dest).OrderBy(r => r.Timestamp);

                        // only keep the first and last record so that the duration calculated on the server will be > 0
                        var firstRecord = records.FirstOrDefault();
                        var lastRecord = records.LastOrDefault();
                        if (firstRecord != null)
                        {
                            sendQueue.Add(firstRecord);
                            // if the last record is a second or more later, add it too
                            if (lastRecord != null && lastRecord != firstRecord &&
                                Convert.ToDateTime(lastRecord.Timestamp) - Convert.ToDateTime(firstRecord.Timestamp)
                                    >= TimeSpan.FromSeconds(1.0))
                            {
                                sendQueue.Add(lastRecord);
                            }
                        }
                    }
                }

                if (sendQueue.Count > 0)
                {
                    // trace the records
                    TraceLog.TraceInfo(String.Format("Sending records: {0}", JsonConvert.SerializeObject(sendQueue)));

                    // send the queue to the web service
                    WebServiceHelper.PostRecords(credentials, sendQueue, null, null);

                    // BUGBUG: need to create a callback that handles failure modes - right now the records are lost if the service is down
                }
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("Send failed", ex);
            }
        }

        #endregion
    }
}
