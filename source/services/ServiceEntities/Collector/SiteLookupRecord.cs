using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MongoRepository;

namespace WebTimer.ServiceEntities.Collector
{
    [CollectionName("CollectorRecords")]
    public class SiteLookupRecord : Entity, ISiteLookupRecord
    {
        // ISiteLookupRecord
        [Display(Name = "Device ID")]
        public string HostMacAddress { get; set; }

        [Display(Name = "Device IP Address")]
        public string HostIpAddress { get; set; }

        [Display(Name = "Device Name")]
        public string HostName { get; set; }

        [Display(Name = "Website Name")]
        public string WebsiteName { get; set; }

        public string Timestamp { get; set; }

        public int Duration { get; set; }

        [Display(Name = "User")]
        public string UserId { get; set; }

        // Concurrency control
        [Display(Name = "State")]
        public string State { get; set; }
    }

    public class RecordState
    {
        public const string New = "New";
        public const string Locked = "Locked";
        public const string Processed = "Processed";
    }
}