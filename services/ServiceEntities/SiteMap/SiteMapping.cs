using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MongoRepository;

namespace ServiceEntities.SiteMap
{
    [CollectionName("SiteMappings")]
    public class SiteMapping : Entity
    {
        public string Site { get; set; }
        public string Category { get; set; }

        public class Categories
        {
            public const string Suppressed = "Suppressed";
            public const string Social = "Social";
            public const string Video = "Video";
            public const string Educational = "Educational";
        };

        public static List<SiteMapping> SiteMappings = new List<SiteMapping>() {
            // social
            new SiteMapping() { Site = "facebook.com", Category = Categories.Social },
            new SiteMapping() { Site = "tumblr.com", Category = Categories.Social },
            new SiteMapping() { Site = "instagram.com", Category = Categories.Social },
            new SiteMapping() { Site = "myspace.com", Category = Categories.Social },
            new SiteMapping() { Site = "twitter.com", Category = Categories.Social },
            new SiteMapping() { Site = "plus.google.com", Category = Categories.Social },
            // video
            new SiteMapping() { Site = "netflix.com", Category = Categories.Video },
            new SiteMapping() { Site = "hulu.com", Category = Categories.Video },
            new SiteMapping() { Site = "youtube.com", Category = Categories.Video },
            // educational
            new SiteMapping() { Site = "khanacademy.org", Category = Categories.Educational },
            // suppressed
            new SiteMapping() { Site = "suppress", Category = Categories.Suppressed },
        };
    }
}