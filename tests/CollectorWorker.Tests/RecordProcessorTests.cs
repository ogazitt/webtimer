using System;
using System.Collections.Generic;
using System.Linq;

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
        public void ValidateSessionProcessing(
            [Range(1,10)] int sessionCount, 
            [Values(10, 50, 100)] int recordCount)
        {
            var macAddress = macAddresses[0];
            var websiteName = websiteNames[0];
            var list = CreateRecordList(macAddress, websiteName, sessionCount, recordCount);

            var sessions = RecordProcessor.ProcessRecords(siteMapRepository, null, list);
            sessions.Count.Should().Be(sessionCount);
        }

        [Test]
        public void ValidateWebSession()
        {
            var macAddress = macAddresses[0];
            var websiteName = websiteNames[0];
            var list = CreateRecordList(macAddress, websiteName, 1, 1);
            var sessions = RecordProcessor.ProcessRecords(siteMapRepository, null, list);
            sessions.Count.Should().Be(1);

            var session = sessions[0];
            session.DeviceId.Should().Be(macAddress);
            session.Duration.Should().BeGreaterOrEqualTo(0);
            session.Start.Should().NotBeNullOrEmpty();
            session.Site.Should().Be(websiteName);
            session.UserId.Should().NotBeNullOrEmpty();

            if (session.Device != null)
            {
                var device = session.Device;
                device.DeviceId.Should().Be(macAddress);
                device.UserId.Should().Be(session.UserId);
            }
        }

        [Test]
        public void OutOfOrderTimestamps()
        {
            var macAddress = macAddresses[0];
            var websiteName = websiteNames[0];
            var list = CreateRecordList(macAddress, websiteName, 1, 2);
            list = list.OrderByDescending(r => r.Timestamp).ToList();
            var sessions = RecordProcessor.ProcessRecords(siteMapRepository, null, list);
            sessions.Count.Should().Be(1);

            var session = sessions[0];
            session.Duration.Should().BeGreaterOrEqualTo(0);
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
                    Timestamp = now.ToString("s"),
                    UserId = macAddress
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
