using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using MongoDB.Driver;
using MongoRepository;

using WebTimer.ServiceEntities.Collector;
using WebTimer.ServiceEntities.UserData;
using WebTimer.ServiceHost;

namespace WebTimer.ServiceHost
{
    public class CollectorContext : MongoRepository<CollectorRecord>
    {
        public CollectorContext() : base(HostEnvironment.MongoUri, HostEnvironment.MongoCollectionName) { }

        public CollectorContext(IPrincipal user)
            : base(HostEnvironment.MongoUri, HostEnvironment.MongoCollectionName)
        {
            UserId = user.Identity.Name;
        }

        public CollectorContext(string userName)
            : base(HostEnvironment.MongoUri, HostEnvironment.MongoCollectionName)
        {
            UserId = userName;
        }

        public string UserId { get; private set; }

        /// <summary>
        /// Add a record to the database
        /// </summary>
        /// <param name="record">a CollectorRecord</param>
        /// <returns>true for success, false for failure</returns>
        public bool AddRecord(CollectorRecord record)
        {
            if (!ValidateRecord(record))
                return false;
            this.Add(record);
            return true;
        }

        /// <summary>
        /// Get all records belonging to this user that need to be processed
        /// This method should only be called from a privileged worker because it
        /// does not filter its results based on a user context
        /// </summary>
        /// <param name="workerName"></param>
        /// <returns></returns>
        public CollectorRecord GetRecordToProcess(string workerName)
        {
            var lockString = string.Format("{0}: {1}", RecordState.Locked, workerName);
            try
            {
                // get the next unprocessed record
                var record = this.FirstOrDefault(r =>
                    // r.UserId == UserId &&   // NOTE THAT THIS DOES NOT FILTER BASED ON USERID
                    r.State == RecordState.New);

                if (record != null)
                {
                    // lock the record
                    record.State = lockString;
                    this.Update(record);

                    // now retrieve the record that we managed to lock
                    record = this.FirstOrDefault(r => r.State == lockString);
                    return record;
                }
                return null;
            }
            catch (MongoConnectionException ex)
            {
                TraceLog.TraceException("Cannot connect to Mongo instance", ex);
                return null;
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("Getting record from Mongo failed", ex);
                return null;
            }
        }

#region Helpers

        private bool ValidateRecord(CollectorRecord record)
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

            if (string.IsNullOrEmpty(record.State))
                record.State = RecordState.New;
            
            return true;
        }

#endregion
    }
}