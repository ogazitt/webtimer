using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebTimer.ServiceEntities.UserData
{
    public class Colors
    {
        public const string Black  = "#000000";
        public const string Blue   = "#0000ff";
        public const string Red    = "#ff0000";
        public const string Green  = "#007f00";
        public const string Yellow = "#ffff00";
        public const string Orange = "#ff7f00";
        public const string Brown  = "#7f0000";
        public const string Purple = "#7f007f";
        public const string Pink   = "#ff7fff";
        
        public static List<string> List = new List<string>() 
        { 
            Colors.Black,
            Colors.Blue, 
            Colors.Red, 
            Colors.Green, 
            Colors.Yellow, 
            Colors.Orange, 
            Colors.Brown, 
            Colors.Purple, 
            Colors.Pink, 
        };
    }
}