using MongoDB.Driver;
using MongoRepository;
using ServiceEntities;
using ServiceEntities.Collector;
using ServiceHost;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;

namespace ServiceHost
{
    public class CollectorContext : MongoRepository<SiteLookupRecord>
    {
        public CollectorContext() : base() {}
        
        public CollectorContext(IPrincipal user)
        {
            UserId = user.Identity.Name;
        }

        public string UserId { get; private set; }

        public List<SiteLookupRecord> GetAllRecords()
        {
            try
            {
                return this.ToList<SiteLookupRecord>();
            }
            catch (MongoConnectionException ex)
            {
                TraceLog.TraceException("Cannot connect to Mongo instance", ex);
                return new List<SiteLookupRecord>();
            }
        }

        public IQueryable<SiteLookupRecord> GetRecordsToProcess(string workerName)
        {
            try
            {
                var list = this.Where(r => r.State == RecordState.New).Take(100);
                foreach (var record in list)
                {
                    record.State = RecordState.Locked;  // BUGBUG: verify that this is our record
                    this.Update(record);
                }
                return list;                
            }
            catch (MongoConnectionException ex)
            {
                TraceLog.TraceException("Cannot connect to Mongo instance", ex);
                return null;
            }
        }
    }
}