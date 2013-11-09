using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebTimer.ServiceEntities.Collector;
using WebTimer.ServiceEntities.SiteMap;
using WebTimer.ServiceEntities.UserData;

namespace WebTimer.Processor
{
    public class RecordProcessor
    {
        const int SessionThreshold = 300;  // sessions elapse after 5 minutes 

        public static List<WebSession> ProcessRecords(SiteMapRepository siteMapRepository, List<WebSession> dbSessions, CollectorRecord collectorRecord)
        {
            // working sessions list
            var sessions = new List<WebSession>();

            // set of WebSession references that have been modified or added
            var resultSessions = new List<WebSession>();
            var recordList = collectorRecord.RecordList.OrderBy(r => r.Timestamp).ToList();

            // terminate all sessions that aren't "in progress"
            var firstRecord = recordList.First();
            var timestamp = Convert.ToDateTime(firstRecord.Timestamp);
            if (dbSessions != null && dbSessions.Count() > 0)
            {
                foreach (var s in dbSessions)
                {
                    var sessionStart = Convert.ToDateTime(s.Start);
                    if (s.InProgress == true && timestamp > sessionStart + TimeSpan.FromSeconds(SessionThreshold))
                    {
                        // session is no longer in progress - mark it for update
                        s.InProgress = false;
                        resultSessions.Add(s);
                    }
                    else
                    {
                        // add it to the working session list
                        sessions.Add(s);
                    }
                }
            }

            foreach (var record in recordList)
            {
                // get the sitemap for this site and skip if the site is suppressed
                var siteMap = siteMapRepository.GetSiteMapping(record.WebsiteName);
                if (siteMap == null)
                    continue;

                // find an in-progress session
                var session = sessions.LastOrDefault(s =>
                    s.Device.DeviceId == collectorRecord.DeviceId &&
                    s.Site == siteMap.Site && 
                    s.InProgress == true);
                if (session == null)
                {
                    var newSession = new WebSession()
                    {
                        UserId = collectorRecord.UserId,
                        DeviceId = collectorRecord.DeviceId,
                        Device = new Device() { DeviceId = collectorRecord.DeviceId, Hostname = collectorRecord.DeviceName, UserId = collectorRecord.UserId },
                        Site = siteMap.Site,
                        Category = siteMap.Category,
                        Start = record.Timestamp,
                        Duration = record.Duration,
                        InProgress = true
                    };
                    sessions.Add(newSession);
                    resultSessions.Add(newSession);
                }
                else
                {
                    var recordTimestamp = Convert.ToDateTime(record.Timestamp);
                    var sessionStart = Convert.ToDateTime(session.Start);
                    var sessionCurrent = sessionStart + TimeSpan.FromSeconds(session.Duration);

                    // is this a new session?
                    if (recordTimestamp > sessionCurrent + TimeSpan.FromSeconds(SessionThreshold))
                    {
                        // terminate the current session and add it to the result session list
                        session.InProgress = false;
                        if (!resultSessions.Contains(session))
                            resultSessions.Add(session);

                        // create a new session
                        var newSession = new WebSession()
                        {
                            UserId = collectorRecord.UserId,
                            DeviceId = collectorRecord.DeviceId,
                            Device = new Device() { DeviceId = collectorRecord.DeviceId, Hostname = collectorRecord.DeviceName, UserId = collectorRecord.UserId },
                            Site = siteMap.Site,
                            Category = siteMap.Category,
                            Start = record.Timestamp,
                            Duration = record.Duration,
                            InProgress = true
                        };
                        sessions.Add(newSession);
                        resultSessions.Add(newSession);
                    }
                    else
                    {
                        // extend the session
                        int duration = 0;
                        if (recordTimestamp < sessionStart)
                        {
                            // handle the case where the current record somehow predates the existing session
                            session.Start = record.Timestamp;
                            sessionStart = recordTimestamp;
                            duration = session.Duration > record.Duration ? session.Duration : record.Duration;
                        }
                        else
                        {
                            // normal processing - just calculate the delta between the new record timestamp and the session start, and add the duration
                            // also ensure that the resulting duration isn't smaller than the existing session duration
                            var delta = (int)(recordTimestamp - sessionStart).TotalSeconds + record.Duration;
                            duration = delta > session.Duration ? delta : session.Duration;
                        }
                        session.Duration = duration;

                        // add it to the result session list if not there already
                        if (!resultSessions.Contains(session))
                            resultSessions.Add(session);
                    }
                }
            }

            return resultSessions;
        }
    }
}
