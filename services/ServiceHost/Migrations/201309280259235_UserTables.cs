namespace ServiceHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserTables : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Categories", "Color", c => c.String());
            AddColumn("dbo.Devices", "DeviceType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Devices", "DeviceType");
            DropColumn("dbo.Categories", "Color");
        }
    }
}
