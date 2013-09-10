using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoRepository;

namespace ServiceEntities.Collector
{
    [CollectionName("CollectorRecords")]
    public class SiteLookupRecord : Entity, ISiteLookupRecord
    {
        // ISiteLookupRecord
        public string HostMacAddress { get; set; }
        public string HostIpAddress { get; set; }
        public string HostName { get; set; }
        public string WebsiteName { get; set; }
        public string Timestamp { get; set; }
        public string UserId { get; set; }

        // Concurrency control
        public string State { get; set; }
    }

    public class RecordState
    {
        public const string New = "New";
        public const string Locked = "Locked";
        public const string Processed = "Processed";
    }
}