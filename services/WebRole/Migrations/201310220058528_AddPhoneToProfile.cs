namespace WebRole.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPhoneToProfile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProfile", "Phone", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserProfile", "Phone");
        }
    }
}
