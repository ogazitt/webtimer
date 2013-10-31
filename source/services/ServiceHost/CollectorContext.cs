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
    public class CollectorContext : MongoRepository<SiteLookupRecord>
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
        /// <param name="record">a SiteLookupRecord</param>
        /// <returns>true for success, false for failure</returns>
        public bool AddRecord(SiteLookupRecord record)
        {
            if (!ValidateRecord(record))
                return false;
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
                if (!ValidateRecord(record))
                    return false;
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
        /// This method should only be called from a privileged worker because it
        /// does not filter its results based on a user context
        /// </summary>
        /// <param name="workerName"></param>
        /// <returns></returns>
        public List<SiteLookupRecord> GetRecordsToProcess(string workerName)
        {
            var lockString = string.Format("{0}: {1}", RecordState.Locked, workerName);
            try
            {
                // get a chunk of new (unprocessed) records
                // BUGBUG: make the record count come from config
                var list = this.Where(r => 
                 // r.UserId == UserId &&   // NOTE THAT THIS DOES NOT FILTER BASED ON USERID
                    r.State == RecordState.New).
                    OrderBy(r => r.Timestamp).
                    Take(100).
                    ToList();

                // lock the records
                foreach (var record in list)
                    record.State = lockString;  

                // update entire list
                this.Update(list);

                // now retrieve the records that we managed to lock
                list = this.Where(r => r.State == lockString).ToList();
                return list;                
            }
            catch (MongoConnectionException ex)
            {
                TraceLog.TraceException("Cannot connect to Mongo instance", ex);
                return null;
            }
        }

        public List<SiteLookupRecord> MockRecords(Device device, int count)
        {
            var rand = new Random(DateTime.Now.Millisecond);
            var now = DateTime.Now;

            var sites = new List<string>()
            { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" };

            // create <count> records
            var list = new List<SiteLookupRecord>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new SiteLookupRecord()
                {
                    HostMacAddress = device.DeviceId,
                    HostIpAddress = device.IpAddress,
                    HostName = device.Hostname,
                    WebsiteName = sites[rand.Next(sites.Count)],
                    Timestamp = now.ToString("s"),
                    UserId = device.UserId,
                    State = RecordState.New
                });
                now += TimeSpan.FromSeconds(30d);
            }
            return list;
        }

#region Helpers

        private bool ValidateRecord(SiteLookupRecord record)
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