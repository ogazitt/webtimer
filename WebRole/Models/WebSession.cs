using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebRole.Models
{
    public class WebSession
    {
        public int WebSessionId { get; set; }

        [Required, MaxLength(30)]
        public string Site { get; set; }

        public string Start { get; set; }
        public string Duration { get; set; }

        [ForeignKey("Device")]
        public int DeviceId { get; set; }
        public virtual Device Device { get; set; }
    }
}