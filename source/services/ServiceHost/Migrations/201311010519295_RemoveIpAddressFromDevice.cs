namespace WebTimer.ServiceHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveIpAddressFromDevice : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Devices", "IpAddress");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Devices", "IpAddress", c => c.String());
        }
    }
}
