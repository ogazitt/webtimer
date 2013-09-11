using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using ServiceHost;

namespace WebRole.Collector.Models
{
    public class RecordRepository : IDisposable
    {
        private MongoServer mongoServer = null;
        private bool disposed = false;

        // private string connectionString = System.Environment.GetEnvironmentVariable("MONGOLAB_URI");
        private string connectionString = HostEnvironment.MongoUri;

        private string dbName = "MongoLab-km";
        private string collectionName = "records";

        public RecordRepository()
        {
        }

        public List<SiteLookupRecord> GetAllRecords()
        {
            try
            {
                MongoCollection<SiteLookupRecord> collection = GetRecordCollection();
                return collection.FindAll().ToList<SiteLookupRecord>();
            }
            catch (MongoConnectionException)
            {
                return new List<SiteLookupRecord>();
            }
        }


        // Creates a Record and inserts it into the collection in MongoDB.
        public void CreateRecord(SiteLookupRecord record)
        {
            MongoCollection<SiteLookupRecord> collection = GetRecordCollection();
            try
            {
                collection.Insert(record);
            }
            catch (MongoCommandException ex)
            {
                string msg = ex.Message;
            }
        }


        private MongoCollection<SiteLookupRecord> GetRecordCollection()
        {
            MongoClient client = new MongoClient(connectionString);
            var server = client.GetServer();
            MongoDatabase database = server.GetDatabase(dbName);
            MongoCollection<SiteLookupRecord> recordCollection = database.GetCollection<SiteLookupRecord>(collectionName);
            return recordCollection;
        }

        #region IDisposable

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (mongoServer != null)
                    {
                        this.mongoServer.Disconnect();
                    }
                }
            }

            this.disposed = true;
        }

        # endregion
    }
}
