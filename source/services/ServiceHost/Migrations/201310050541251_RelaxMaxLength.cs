namespace WebTimer.ServiceHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RelaxMaxLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Devices", "Name", c => c.String());
            AlterColumn("dbo.People", "Name", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.People", "Name", c => c.String(nullable: false, maxLength: 30));
            AlterColumn("dbo.Devices", "Name", c => c.String(nullable: false, maxLength: 30));
        }
    }
}
