// <auto-generated />
namespace ServiceHost.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    public sealed partial class BirthdateToString : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(BirthdateToString));
        
        string IMigrationMetadata.Id
        {
            get { return "201309280444155_BirthdateToString"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return Resources.GetString("Target"); }
        }
    }
}