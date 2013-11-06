using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebTimer.ServiceEntities.UserData;
using WebTimer.ServiceHost;

namespace WebTimer.AnalyticsWorker
{
    public class AnalyticsWorker : IWorker
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
                    timeout = ConfigurationSettings.GetAsNullableInt(HostEnvironment.ProcessorWorkerTimeoutConfigKey);
                    if (timeout == null)
                        timeout = 60 * 60 * 1000;  // default to 60 minutes
                    else
                        timeout *= 1000;  // convert to ms
                }
                return timeout.Value;
            }
        }

        public void Start()
        {
            while (true)
            {
                try
                {
                    var context = Storage.NewUserDataContext;

                    var today = DateTime.Now.Date;
                    var currentRecord = context.Snapshots.FirstOrDefault(s => s.Date == today) ??
                        new StatSnapshot() { Date = today };

                    currentRecord.Devices = context.Devices.Count();
                    currentRecord.People = context.People.Count();
                    currentRecord.Users = context.Devices.Select(d => d.UserId).Distinct().Count();

                    context.SaveChanges();

                    TraceLog.TraceInfo("Updated statistics");
                }
                catch (Exception ex)
                {
                    TraceLog.TraceException("Analytics processing failed", ex);
                }

                // sleep for the timeout period
                Thread.Sleep(Timeout);
            }
        }
    }
}
