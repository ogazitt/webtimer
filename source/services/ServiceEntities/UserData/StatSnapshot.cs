using System;
using System.ComponentModel.DataAnnotations;

namespace WebTimer.ServiceEntities.UserData
{
    public class StatSnapshot
    {
        [Required, Key]
        public DateTime Date { get; set; }
        public int Users { get; set; }
        public int Devices { get; set; }
        public int People { get; set; }
    }
}
