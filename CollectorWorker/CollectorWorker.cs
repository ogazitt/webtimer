using ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CollectorWorker
{
    public class CollectorWorker : IWorker
    {
        public static string Me
        {
            get { return String.Concat(Environment.MachineName.ToLower(), "-", Thread.CurrentThread.ManagedThreadId.ToString()); }
        }

        private int? timeout;
        public int Timeout
        {
            get
            {
                if (!timeout.HasValue)
                {
                    timeout = ConfigurationSettings.GetAsNullableInt(HostEnvironment.CollectorWorkerTimeoutConfigKey);
                    if (timeout == null)
                        timeout = 5 * 60 * 1000;  // default to 5 minutes
                    else
                        timeout *= 1000;  // convert to ms
                }
                return timeout.Value;
            }
        }

        public void Start()
        {
            // run an infinite loop doing the following:
            //   grab records from the collector database
            //   process records into session records in the user database
            //   remove the processed records
            //   sleep for the timeout period
            while (true)
            {
                var UserContext = Storage.NewUserContext;

                try
                {
                    DateTime now = DateTime.UtcNow;
                    //var timers = SuggestionsContext.Timers.Where(t => t.NextRun < now).ToList();


                }
                catch (Exception ex)
                {
                    TraceLog.TraceException("Collector processing failed", ex);
                }

                // sleep for the timeout period
                Thread.Sleep(Timeout);
            }
        }
    }
}

