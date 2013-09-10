using System;
using System.Collections.Generic;

using FluentAssertions;
using Moq;
using NUnit.Framework;

using Collector;
using ServiceEntities.Collector;

namespace CollectorWorker.Tests
{
    /// <summary>
    /// Summary description for CollectorTest
    /// </summary>
    [TestFixture]
    public class RecordProcessorTests : TestBase
    {
        [Test]
        public void Session(
            [Range(1,10)] int sessionCount, 
            [Values(10, 50, 100)] int recordCount)
        {
            var macAddress = macAddresses[0];
            var websiteName = websiteNames[0];
            var list = CreateRecordList(macAddress, websiteName, sessionCount, recordCount);

            var sessions = RecordProcessor.ProcessRecords(null, list);
            sessions.Count.Should().Be(sessionCount);
        }

        private List<ISiteLookupRecord> CreateRecordList(string macAddress, string websiteName, int sessionCount, int recordCount)
        {
            var now = DateTime.Now;
            var list = new List<ISiteLookupRecord>();
            var ipAddress = ipAddresses[macAddress];

            int sessionBreak = recordCount / sessionCount;
            int runningSessionCount = 1;

            for (int i = 1; i <= recordCount; i++)
            {
                list.Add(new SiteLookupRecord()
                {
                    HostMacAddress = macAddress,
                    HostIpAddress = ipAddress,
                    HostName = ipAddress,
                    WebsiteName = websiteName,
                    Timestamp = now.ToString()
                });
                if (i % sessionBreak == 0 && runningSessionCount < sessionCount)
                {
                    now += TimeSpan.FromMinutes(10.0);
                    runningSessionCount++;
                }
                else
                    now += TimeSpan.FromMinutes(2.0);
            }
            return list;
        }
    }
}
