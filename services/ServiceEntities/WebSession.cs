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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int  WebSessionId { get; set; }

        [Required]
        public string Site       { get; set; }

        public string Start      { get; set; } // string rep of yyyy-mm-dd hh:MM:ss
        public int    Duration   { get; set; } // in seconds
        public bool   InProgress { get; set; } // is the session in progress or not

        [Required, ForeignKey("Device")]
        public string  DeviceId  { get; set; }
        public virtual Device Device { get; set; }

        // this is the account the person belongs to
        [Required]
        public string UserId { get; set; }        
    }
}