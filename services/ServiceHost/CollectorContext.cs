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

        /// <summary>
        /// Add a record to the database
        /// </summary>
        /// <param name="record">a SiteLookupRecord</param>
        /// <returns>true for success, false for failure</returns>
        public bool AddRecord(SiteLookupRecord record)
        {
            if (string.IsNullOrEmpty(record.UserId))
                record.UserId = UserId;

            if (UserId != record.UserId)
            {
                TraceLog.TraceError(String.Format(
                    "UserId in record {0} does not match current user {1}",
                    record.UserId,
                    UserId));
                return false;
            }
            this.Add(record);
            return true;
        }

        /// <summary>
        /// Add multiple records at the same time using the same connection
        /// </summary>
        /// <param name="records">a list of SiteLookupRecords</param>
        /// <returns>true for success, false for failure</returns>
        public bool AddRecords(List<SiteLookupRecord> records)
        {
            foreach (var record in records)
            {
                if (string.IsNullOrEmpty(record.UserId))
                    record.UserId = UserId;

                if (UserId != record.UserId)
                {
                    TraceLog.TraceError(String.Format(
                        "UserId in record {0} does not match current user {1}",
                        record.UserId,
                        UserId));
                    return false;
                }
            }
            this.Add(records);
            return true;
        }

        /// <summary>
        /// Get all the records belonging to this User
        /// </summary>
        /// <returns></returns>
        public List<SiteLookupRecord> GetAllRecords()
        {
            try
            {
                return this.Where(r =>
                    r.UserId == UserId).
                    ToList<SiteLookupRecord>();
            }
            catch (MongoConnectionException ex)
            {
                TraceLog.TraceException("Cannot connect to Mongo instance", ex);
                return new List<SiteLookupRecord>();
            }
        }

        /// <summary>
        /// Get all records belonging to this user that need to be processed
        /// </summary>
        /// <param name="workerName"></param>
        /// <returns></returns>
        public IQueryable<SiteLookupRecord> GetRecordsToProcess(string workerName)
        {
            try
            {
                var list = this.Where(r => 
                    r.UserId == UserId &&
                    r.State == RecordState.New).
                    Take(100);
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