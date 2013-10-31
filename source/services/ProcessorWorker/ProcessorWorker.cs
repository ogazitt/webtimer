using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using WebTimer.Processor;
using WebTimer.ServiceEntities.Collector;
using WebTimer.ServiceEntities.UserData;
using WebTimer.ServiceHost;

namespace WebTimer.ProcessorWorker
{
    public class ProcessorWorker : IWorker
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
                        timeout = 5 * 60 * 1000;  // default to 5 minutes
                    else
                        timeout *= 1000;  // convert to ms
                }
                return timeout.Value;
            }
        }

        public void Start()
        {
            // use a persistent site map repository for the duration of worker lifetime

            TraceLog.TraceDetail("Initializing Site Map Repository");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var SiteMapRepository = Storage.NewSiteMapRepository;
            var versionString = SiteMapRepository.Initialize(Me);

            stopwatch.Stop();
            TraceLog.TraceDetail(string.Format(
                "Initialized Site Map Repository version {0}: {1:F2} secs",
                versionString,
                stopwatch.ElapsedMilliseconds / 1000));

            // run an infinite loop doing the following:
            //   grab records from the collector database
            //   process records into session records in the user database
            //   mark the processed records
            //   sleep for the timeout period
            while (true)
            {
                // reinitialize the mapping database if the version changed
                if (SiteMapRepository.VersionChanged())
                {
                    stopwatch.Start();
                    versionString = SiteMapRepository.Initialize(Me);
                    stopwatch.Stop();
                    TraceLog.TraceInfo(string.Format(
                        "Re-Initialized Site Map Repository version {0}: {1:F2} secs",
                        versionString,
                        stopwatch.ElapsedMilliseconds / 1000));
                }

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
                            var workingSessions = RecordProcessor.ProcessRecords(
                                SiteMapRepository, 
                                sessions.ToList<WebSession>(), 
                                newRecords.ToList<ISiteLookupRecord>());

                            // process the resultant sessions
                            foreach (var session in workingSessions)
                            {
                                if (session.WebSessionId == 0)
                                {   // new session

                                    // find out whether the device already exists
                                    var device = UserContext.Devices.FirstOrDefault(d => d.DeviceId == session.DeviceId);
                                    if (device == null)
                                    {
                                        try
                                        {
                                            // create a new device and save it immediately
                                            device = UserDataContext.CreateDeviceFromSession(session);
                                            UserContext.Devices.Add(device);
                                            UserContext.SaveChanges();
                                            TraceLog.TraceInfo(string.Format("Added device {0} to user {1}; Device dump: {2}", device.Name, userId, JsonSerializer.Serialize(device)));
                                        }
                                        catch (DbEntityValidationException ex)
                                        {
                                            var str = new StringBuilder("Failed to create new device.  Validation errors:\n");
                                            foreach (var errors in ex.EntityValidationErrors)
                                                foreach (var result in errors.ValidationErrors)
                                                    str.AppendLine(string.Format("\tProperty: {0}; Error: {1}", result.PropertyName, result.ErrorMessage));
                                            TraceLog.TraceException(str.ToString(), ex);
                                            TraceLog.TraceError(string.Format("Session dump: {0}; Device dump: {1}", JsonSerializer.Serialize(session), JsonSerializer.Serialize(device)));
                                            // fail fast
                                            throw;
                                        }
                                    }
                                    else
                                    {
                                        // ensure device belongs to current user
                                        if (device.UserId != userId)
                                        {
                                            TraceLog.TraceError(string.Format(
                                                "Possible security issue: User {0} is trying to claim Device {0} that already belongs to User {2}",
                                                userId,
                                                session.DeviceId,
                                                device.UserId));
                                            
                                            // TODO: do something smarter than ignoring the record
                                            continue;
                                        }
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
                            
                            // save all the sessions 
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
#if KILL                    // if still debugging this codepath, reset the collector records to New.  BUGBUG - need to harden this with poison message semantics

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

