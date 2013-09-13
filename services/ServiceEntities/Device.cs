﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceEntities
{
    public class Device
    {
        [Key, MaxLength(12)]
        public string DeviceId { get; set; }

        // user-assigned friendly name for the device
        [Required, MaxLength(30)]
        public string Name { get; set; }

        public string Hostname { get; set; }
        public string IpAddress { get; set; }

        [ForeignKey("Person")]
        public int? PersonId { get; set; }
        public virtual Person Person { get; set; }

        // this is the account the device belongs to
        [Required]
        public string UserId { get; set; }
    }
}