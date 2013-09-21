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

        /// <summary>
        /// Start the upload client
        /// </summary>
        public static void Start()
        {
            // start the uploader thread if running in production
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
            // terminate the loop
            uploadFlag = false;
            
            // one last send
            Send();
        }

        #region Helpers

        private static void SendLoop()
        {
            while (uploadFlag)
            {
                Send();
                
                // sleep for a minute
                Thread.Sleep(60000);
            }
        }

        private static void Send()
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
                    var record = recordList.FirstOrDefault(r => r.HostName == host && r.WebsiteName == dest);
                    if (record != null)
                        sendQueue.Add(record);
                }
            }

            if (sendQueue.Count > 0)
            {
                // send the queue to the web service
                WebServiceHelper.PostRecords(new User { Name = "ogazitt", Password = "zrc022.." }, sendQueue, null, null);

                // BUGBUG: need to create a callback that handles failure modes - right now the records are lost if the service is down
            }
        }

        #endregion
    }
}
