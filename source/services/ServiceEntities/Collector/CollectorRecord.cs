using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MongoRepository;

namespace WebTimer.ServiceEntities.Collector
{
    [CollectionName("CollectorRecords")]
    public class CollectorRecord : Entity
    {
        [Display(Name = "Device ID")]
        public string DeviceId { get; set; }

        [Display(Name = "Device Name")]
        public string DeviceName { get; set; }

        [Display(Name = "User")]
        public string UserId { get; set; }

        // Concurrency control
        [Display(Name = "State")]
        public string State { get; set; }

        public IList<SiteLookupRecord> RecordList { get; set; }
    }

    public class RecordState
    {
        public const string New = "New";
        public const string Locked = "Locked";
        public const string Processed = "Processed";
    }

    // this is the contract between the client JSON payload and the service
    public class CollectorFields
    {
        public const string DeviceId = "DeviceId";
        public const string DeviceName = "DeviceName";
        public const string Records = "Records";
        public const string SoftwareVersion = "SoftwareVersion";
    }

    public class ServiceResponse
    {
        public int RecordsProcessed { get; set; }
        public ControlMessage ControlMessage { get; set; }
    }

    public enum ControlMessage
    {
        Normal = 0,
        SuspendCollection = 1,
        DisableDevice = 2
    }
}