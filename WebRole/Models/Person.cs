using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebRole.Models
{
    public class Person
    {
        public int PersonId { get; set; }

        [Required, MaxLength(30)]
        public string Name { get; set; }

        public bool IsChild { get; set; }
        public DateTime Birthdate { get; set; }

        // this is the account the person belongs to
        [Required]
        public string UserId { get; set; }
    }
}