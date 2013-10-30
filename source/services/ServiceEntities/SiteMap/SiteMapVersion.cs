using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoRepository;

namespace ServiceEntities.SiteMap
{
    public class SiteMapVersion : Entity
    {
        public string VersionString { get; set; }
        public string Status { get; set; }

        // Status values
        public const string Corrupted = "Corrupted";
        public const string OK = "OK";
    }
}
