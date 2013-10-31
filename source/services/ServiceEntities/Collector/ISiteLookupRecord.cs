using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebTimer.ServiceEntities.Collector
{
    public interface ISiteLookupRecord
    {
        string HostMacAddress { get; set; }
        string HostIpAddress { get; set; }
        string HostName { get; set; }
        string WebsiteName { get; set; }
        string Timestamp { get; set; }
        int    Duration { get; set; }
        string UserId { get; set; }
    }
}