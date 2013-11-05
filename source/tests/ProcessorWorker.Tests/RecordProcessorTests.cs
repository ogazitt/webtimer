using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
using Moq;
using NUnit.Framework;

using WebTimer.Processor;
using WebTimer.ServiceEntities.Collector;

namespace WebTimer.CollectorWorker.Tests
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
            var record = CreateRecordList(macAddress, websiteName, sessionCount, recordCount);

            var sessions = RecordProcessor.ProcessRecords(siteMapRepository, null, record);
            sessions.Count.Should().Be(sessionCount);
        }

        [Test]
        public void ValidateWebSession()
        {
            var macAddress = macAddresses[0];
            var websiteName = websiteNames[0];
            var record = CreateRecordList(macAddress, websiteName, 1, 1);
            var sessions = RecordProcessor.ProcessRecords(siteMapRepository, null, record);
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
            var record = CreateRecordList(macAddress, websiteName, 1, 2);
            record.RecordList = record.RecordList.OrderByDescending(r => r.Timestamp).ToList();
            var sessions = RecordProcessor.ProcessRecords(siteMapRepository, null, record);
            sessions.Count.Should().Be(1);

            var session = sessions[0];
            session.Duration.Should().BeGreaterOrEqualTo(0);
        }

        private CollectorRecord CreateRecordList(string macAddress, string websiteName, int sessionCount, int recordCount)
        {
            var now = DateTime.Now;
            var record = new CollectorRecord()
            {
                DeviceId = macAddress,
                DeviceName = macAddress,
                UserId = macAddress,
                RecordList = new List<SiteLookupRecord>()
            };

            int sessionBreak = recordCount / sessionCount;
            int runningSessionCount = 1;

            for (int i = 1; i <= recordCount; i++)
            {
                record.RecordList.Add(new SiteLookupRecord()
                {
                    WebsiteName = websiteName,
                    Timestamp = now.ToString("s"),
                    Duration = 0
                });
                if (i % sessionBreak == 0 && runningSessionCount < sessionCount)
                {
                    now += TimeSpan.FromMinutes(10.0);
                    runningSessionCount++;
                }
                else
                    now += TimeSpan.FromMinutes(2.0);
            }
            return record;
        }
    }
}
