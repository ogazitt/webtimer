using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MongoRepository;

namespace WebTimer.ServiceEntities.Collector
{
    public class SiteLookupRecord //: ISiteLookupRecord
    {
        // ISiteLookupRecord
        [Display(Name = "Website Name")]
        public string WebsiteName { get; set; }

        public string Timestamp { get; set; }

        public int Duration { get; set; }
    }
}