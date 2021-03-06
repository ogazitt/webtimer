namespace WebTimer.ServiceHost.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using WebTimer.ServiceEntities.UserData;

    internal sealed class Configuration : DbMigrationsConfiguration<WebTimer.ServiceHost.UserDataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(WebTimer.ServiceHost.UserDataContext context)
        {
            //  This method will be called after migrating to the latest version.
            TraceLog.TraceInfo("Seeding Categories in UserData database");
            context.Categories.AddOrUpdate<Category>(c => c.Name, Category.GetCategories().ToArray());
        }
    }
}
