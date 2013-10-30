using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MongoRepository;

namespace ServiceEntities.SiteMap
{
    [CollectionName("UnknownSite")]
    public class UnknownSite : Entity
    {
        public string Site { get; set; }
    }
}