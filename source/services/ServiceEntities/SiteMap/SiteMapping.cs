using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MongoRepository;
using WebTimer.ServiceEntities;

namespace WebTimer.ServiceEntities.SiteMap
{
    [CollectionName("SiteMappings")]
    public class SiteMapping : Entity
    {
        public string Site { get; set; }
        public string Category { get; set; }

        public static List<SiteMapping> SiteMappings = new List<SiteMapping>() {
            // social
            new SiteMapping() { Site = "facebook.com", Category = UserData.Category.Categories.Social },
            new SiteMapping() { Site = "tumblr.com", Category = UserData.Category.Categories.Social },
            new SiteMapping() { Site = "instagram.com", Category = UserData.Category.Categories.Social },
            new SiteMapping() { Site = "myspace.com", Category = UserData.Category.Categories.Social },
            new SiteMapping() { Site = "twitter.com", Category = UserData.Category.Categories.Social },
            new SiteMapping() { Site = "plus.google.com", Category = UserData.Category.Categories.Social },
            // video
            new SiteMapping() { Site = "netflix.com", Category = UserData.Category.Categories.Video },
            new SiteMapping() { Site = "hulu.com", Category = UserData.Category.Categories.Video },
            new SiteMapping() { Site = "youtube.com", Category = UserData.Category.Categories.Video },
            // educational
            new SiteMapping() { Site = "khanacademy.org", Category = UserData.Category.Categories.Educational },
            new SiteMapping() { Site = "lumosity.com", Category = UserData.Category.Categories.Educational },
            // suppressed
            new SiteMapping() { Site = "suppress", Category = UserData.Category.Categories.Suppressed },
        };
    }
}