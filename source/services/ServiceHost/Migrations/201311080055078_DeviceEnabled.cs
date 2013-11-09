namespace WebTimer.ServiceHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeviceEnabled : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Devices", "Enabled", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Devices", "Enabled");
        }
    }
}
