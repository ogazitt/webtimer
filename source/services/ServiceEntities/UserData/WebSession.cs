using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebTimer.ServiceEntities.UserData
{
    public class WebSession
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int  WebSessionId { get; set; }

        [Required]
        public string Site       { get; set; }

        public string Start      { get; set; } // string rep of yyyy-mm-dd hh:MM:ss
        
        public int    Duration   { get; set; } // in seconds

        [Display(Name = "In Progress?")]
        public bool   InProgress { get; set; } // is the session in progress or not

        public string Category   { get; set; }

        [Required, ForeignKey("Device")]
        public string  DeviceId  { get; set; }
        public virtual Device Device { get; set; }

        // this is the account the person belongs to
        [Required, Display(Name = "User")]
        public string UserId { get; set; }        
    }
}