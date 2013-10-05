using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceEntities.UserData
{
    public class Person
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PersonId { get; set; }

        [Required]
        public string Name { get; set; }

        // this is the account the person belongs to
        [Required]
        public string UserId { get; set; }

        public bool IsChild { get; set; }

        public string Birthdate { get; set; }

        public string Color { get; set; }
    }
}