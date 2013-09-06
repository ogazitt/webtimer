using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace WebRole.Collector.Models
{
    public class Record
    {
        [BsonId(IdGenerator = typeof(CombGuidGenerator))]
        public Guid Id { get; set; }

        [BsonElement("HostMacAddress")]
        public string HostMacAddress { get; set; }

        [BsonElement("Timestamp")]
        public string Timestamp { get; set; }
    }
}