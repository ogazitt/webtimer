using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;

namespace WebRole.Collector.Models
{
    public class SiteLookupRecord : Record
    {
        [BsonElement("Sitename")]
        public string Sitename { get; set; }
    }
}