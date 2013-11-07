using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebTimer.ServiceEntities.UserData
{
    public class Category
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        [Required, MaxLength(30)]
        public string Name { get; set; }

        public string Color { get; set; }

        public class Categories
        {
            public const string Educational = "Educational";
            public const string Email = "Email";
            public const string Entertainment = "Entertainment";
            public const string Retail = "Retail";
            public const string Social = "Social";
            public const string Suppressed = "Suppressed";
            public const string Video = "Video";
        };

        public static List<Category> GetCategories()
        {
            return new List<Category>()
            {
                new Category() { Name = Categories.Social, Color = Colors.Red },
                new Category() { Name = Categories.Video, Color = Colors.Blue },
                new Category() { Name = Categories.Educational, Color = Colors.Green },
                new Category() { Name = Categories.Entertainment, Color = Colors.Yellow },
                new Category() { Name = Categories.Email, Color = Colors.Orange },
                new Category() { Name = Categories.Retail, Color = Colors.Purple },
            };
        }
    }
}