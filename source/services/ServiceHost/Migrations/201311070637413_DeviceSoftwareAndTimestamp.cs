namespace WebTimer.ServiceHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeviceSoftwareAndTimestamp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Devices", "SoftwareVersion", c => c.String());
            AddColumn("dbo.Devices", "Timestamp", c => c.DateTime(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Devices", "Timestamp");
            DropColumn("dbo.Devices", "SoftwareVersion");
        }
    }
}
