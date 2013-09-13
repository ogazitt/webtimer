using Collector;
using ServiceEntities;
using ServiceEntities.Collector;
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
            //   mark the processed records
            //   sleep for the timeout period
            while (true)
            {
                var UserContext = Storage.NewUserDataContext;
                var CollectorContext = Storage.NewCollectorContext;

                try
                {
                    DateTime now = DateTime.UtcNow;

                    var newRecords = CollectorContext.GetRecordsToProcess(Me);
                    if (newRecords.Count() > 0)
                    {
                        try
                        {
                            // get all the in-progress sessions belonging to the userid
                            var userId = newRecords.First().UserId;
                            var sessions = UserContext.WebSessions.Where(ws => ws.Device.UserId == userId).OrderBy(ws => ws.Start);

                            // process the records against the existing sessions
                            var workingSessions = RecordProcessor.ProcessRecords(sessions.ToList<WebSession>(), newRecords.ToList<ISiteLookupRecord>());

                            // process the resultant sessions
                            foreach (var session in workingSessions)
                            {
                                if (session.WebSessionId == 0)
                                {   // new session

                                    // find out whether the device already exists
                                    var device = UserContext.Devices.FirstOrDefault(d => d.UserId == userId && d.DeviceId == session.DeviceId);
                                    if (device == null)
                                    {
                                        // create a new device
                                        device = UserDataContext.CreateDeviceFromSession(session);
                                        UserContext.Devices.Add(device);
                                    }

                                    // create a new session with the correct device
                                    session.Device = device;
                                    UserContext.WebSessions.Add(session);
                                }
                                else
                                {
                                    var sessionToModify = sessions.Single(s => s.WebSessionId == session.WebSessionId);
                                    sessionToModify.Duration = session.Duration;
                                    sessionToModify.InProgress = session.InProgress;
                                }
                            }
                            UserContext.SaveChanges();

                            // mark the records as processed using batch semantics
                            foreach (var record in newRecords)
                                record.State = RecordState.Processed;
                            CollectorContext.Update(newRecords);

                            TraceLog.TraceInfo(string.Format("Processed {0} records for user {1}", newRecords.Count(), userId));
                        }
                        catch (Exception ex)
                        {
                            TraceLog.TraceException("Could not save session changes", ex);
#if DEBUG                   // if still debugging this codepath, reset the collector records to New.  BUGBUG - need to harden this with poison message semantics

                            // mark the records as new using batch semantics
                            foreach (var record in newRecords)
                                record.State = RecordState.New;
                            CollectorContext.Update(newRecords);
#endif
                        }

                        // keep reading records until they are all processed
                        continue;
                    }
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

