using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoRepository;

namespace ServiceEntities.Collector
{
    public interface ISiteLookupRecord
    {
        string HostMacAddress { get; set; }
        string HostIpAddress { get; set; }
        string HostName { get; set; }
        string WebsiteName { get; set; }
        string Timestamp { get; set; }
        string UserId { get; set; }
    }
}