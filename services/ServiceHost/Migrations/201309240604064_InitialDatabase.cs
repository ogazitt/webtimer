namespace ServiceHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 30),
                    })
                .PrimaryKey(t => t.CategoryId);
            
            CreateTable(
                "dbo.Devices",
                c => new
                    {
                        DeviceId = c.String(nullable: false, maxLength: 12),
                        Name = c.String(nullable: false, maxLength: 30),
                        Hostname = c.String(),
                        IpAddress = c.String(),
                        PersonId = c.Int(),
                        UserId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.DeviceId)
                .ForeignKey("dbo.People", t => t.PersonId)
                .Index(t => t.PersonId);
            
            CreateTable(
                "dbo.People",
                c => new
                    {
                        PersonId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 30),
                        UserId = c.String(nullable: false),
                        IsChild = c.Boolean(nullable: false),
                        Birthdate = c.DateTime(),
                        Color = c.String(),
                    })
                .PrimaryKey(t => t.PersonId);
            
            CreateTable(
                "dbo.WebSessions",
                c => new
                    {
                        WebSessionId = c.Int(nullable: false, identity: true),
                        Site = c.String(nullable: false),
                        Start = c.String(),
                        Duration = c.Int(nullable: false),
                        InProgress = c.Boolean(nullable: false),
                        Category = c.String(),
                        DeviceId = c.String(nullable: false, maxLength: 12),
                        UserId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.WebSessionId)
                .ForeignKey("dbo.Devices", t => t.DeviceId, cascadeDelete: true)
                .Index(t => t.DeviceId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.WebSessions", new[] { "DeviceId" });
            DropIndex("dbo.Devices", new[] { "PersonId" });
            DropForeignKey("dbo.WebSessions", "DeviceId", "dbo.Devices");
            DropForeignKey("dbo.Devices", "PersonId", "dbo.People");
            DropTable("dbo.WebSessions");
            DropTable("dbo.People");
            DropTable("dbo.Devices");
            DropTable("dbo.Categories");
        }
    }
}
