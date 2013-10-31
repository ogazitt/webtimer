namespace WebTimer.ServiceHost.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BirthdateToString : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.People", "Birthdate", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.People", "Birthdate", c => c.DateTime());
        }
    }
}
