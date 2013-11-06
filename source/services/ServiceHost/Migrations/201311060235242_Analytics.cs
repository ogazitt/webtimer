namespace WebTimer.ServiceHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Analytics : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StatSnapshots",
                c => new
                    {
                        Date = c.DateTime(nullable: false),
                        Users = c.Int(nullable: false),
                        Devices = c.Int(nullable: false),
                        People = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Date);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.StatSnapshots");
        }
    }
}
