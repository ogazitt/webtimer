using System.Collections.Generic;

namespace WebTimer.Client.Models
{
    public class RecordList
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string SoftwareVersion { get; set; }
        public List<Record> Records { get; set; }
    }
}
