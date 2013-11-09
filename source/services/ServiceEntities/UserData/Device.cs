using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Web;

namespace WebTimer.ServiceEntities.UserData
{
    public class Device
    {
        [Key, MaxLength(12), Display(Name = "Device ID")]
        public string DeviceId { get; set; }

        // user-assigned friendly name for the device
        [Display(Name = "Device")]
        public string Name { get; set; }

        [Display(Name = "Host Name")]
        public string Hostname { get; set; }

        [Display(Name = "Device Type")]
        public string DeviceType { get; set; }

        public static class DeviceTypes
        {
            public const string Computer = "Computer";
            public const string Phone = "Phone";
            public const string Tablet = "Tablet";
        }

        [Display(Name = "Version")]
        public string SoftwareVersion { get; set; }

        [Display(Name = "Enabled")]
        public bool? Enabled { get; set; }

        [Display(Name = "Timestamp")]
        public DateTime? Timestamp { get; set; }

        [ForeignKey("Person")]
        public int? PersonId { get; set; }
        public virtual Person Person { get; set; }

        // this is the account the device belongs to
        [Required, Display(Name = "User")]
        public string UserId { get; set; }

        private const string hexDigit = "0123456789ABCDEF";

        public Device() { }

        public static Device CreateNewDevice(string userName)
        {
            var deviceId = CreateDeviceId();
            return new Device()
            {
                DeviceId = deviceId,
                Hostname = deviceId,
                Name = string.Format("{0}'s device", userName),
                Enabled = true,
                UserId = userName
            };
        }

        public static string CreateDeviceId()
        {
            var rand = new Random(DateTime.Now.Millisecond);

            var sb = new StringBuilder();
            for (int i = 0; i < 12; i++)
                sb.Append(hexDigit.Substring(rand.Next(hexDigit.Length), 1));
            var addr = sb.ToString();
            return addr;
        }
    }
}