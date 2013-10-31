using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebTimer.ServiceEntities.Collector;

namespace WebTimer.CollectorWorker.Tests
{
    // mocked SiteLookupRecord, so that the test project doesn't have to link with the MongoRepository
    public class SiteLookupRecord : ISiteLookupRecord
    {
        public string HostMacAddress { get; set; }
        public string HostIpAddress { get; set; }
        public string HostName { get; set; }
        public string WebsiteName { get; set; }
        public string Timestamp { get; set; }
        public int    Duration { get; set; }
        public string UserId { get; set; }
    }
}