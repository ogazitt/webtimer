namespace WebTimer.WebRole.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdminFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProfile", "IsAdmin", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserProfile", "PermissionToImpersonate", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserProfile", "PermissionToImpersonate");
            DropColumn("dbo.UserProfile", "IsAdmin");
        }
    }
}
