using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceEntities
{
    public class WebSession
    {
        public int  WebSessionId { get; set; }

        [Required, MaxLength(30)]
        public string Site       { get; set; }

        public string Start      { get; set; } // string rep of yyyy-mm-dd hh:MM:ss
        public int    Duration   { get; set; } // in seconds
        public bool   InProgress { get; set; } // is the session in progress or not

        [ForeignKey("Device")]
        public int    DeviceId   { get; set; }
        public virtual Device Device { get; set; }
    }
}