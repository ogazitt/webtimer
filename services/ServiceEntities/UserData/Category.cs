using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ServiceEntities.UserData
{
    public class Category
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        [Required, MaxLength(30)]
        public string Name { get; set; }

        public class Categories
        {
            public const string Suppressed = "Suppressed";
            public const string Social = "Social";
            public const string Video = "Video";
            public const string Educational = "Educational";
        };

        public static List<Category> GetCategories()
        {
            return new List<Category>()
            {
                new Category() { Name = Categories.Social },
                new Category() { Name = Categories.Video },
                new Category() { Name = Categories.Educational },
            };
        }
    }
}