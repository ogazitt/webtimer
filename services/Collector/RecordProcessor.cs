using ServiceEntities;
using ServiceEntities.Collector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector
{
    public class RecordProcessor
    {
        const int SessionThreshold = 300;

        public static List<WebSession> ProcessRecords(List<WebSession> sessions, List<ISiteLookupRecord> recordList)
        {
            if (sessions == null)
                sessions = new List<WebSession>();

            // set of WebSession references that have been modified or added
            var resultSessions = new List<WebSession>();
            recordList = recordList.OrderBy(r => r.UserId).ThenBy(r => r.Timestamp).ToList();

            foreach (var record in recordList)
            {
                var session = sessions.LastOrDefault(s =>
                    s.Device.DeviceId == record.HostMacAddress &&
                    s.Site == record.WebsiteName && 
                    s.InProgress == true);
                if (session == null)
                {
                    var newSession = new WebSession()
                    {
                        UserId = record.UserId,
                        DeviceId = record.HostMacAddress,
                        Device = new Device() { DeviceId = record.HostMacAddress, IpAddress = record.HostIpAddress, Hostname = record.HostName, UserId = record.UserId },
                        Site = record.WebsiteName,
                        Start = record.Timestamp,
                        Duration = 0,
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
                            UserId = record.UserId,
                            DeviceId = record.HostMacAddress,
                            Device = new Device() { DeviceId = record.HostMacAddress, IpAddress = record.HostIpAddress, Hostname = record.HostName, UserId = record.UserId },
                            Site = record.WebsiteName,
                            Start = record.Timestamp,
                            Duration = 0,
                            InProgress = true
                        };
                        sessions.Add(newSession);
                        resultSessions.Add(newSession);
                    }
                    else
                    {
                        // extend the session
                        session.Duration = (int) (recordTimestamp - sessionStart).TotalSeconds;

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
