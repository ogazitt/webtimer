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

        [Required, MaxLength(30)]
        public string Name { get; set; }

        // this is the account the person belongs to
        [Required]
        public string UserId { get; set; }

        public bool IsChild { get; set; }
        public DateTime? Birthdate { get; set; }

        public string Color { get; set; }

        public static class Colors
        {
            public const string Blue = "blue";
            public const string Red = "red";
            public const string Green = "green";
            public const string Yellow = "yellow";
            public const string Orange = "orange";
            public const string Brown = "brown";
            public const string Purple = "purple";
            public const string Black = "black";
        }

        public static List<string> ColorList = new List<string>() 
        { 
            Colors.Blue, 
            Colors.Red, 
            Colors.Green, 
            Colors.Yellow, 
            Colors.Orange, 
            Colors.Brown, 
            Colors.Purple, 
            Colors.Black
        };
    }
}