namespace WebTimer.WebRole.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNameToUserProfile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProfile", "Name", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.UserProfile", "Name");
        }
    }
}
