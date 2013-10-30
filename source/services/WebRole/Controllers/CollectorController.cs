using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http;
using Newtonsoft.Json.Linq;

using ServiceEntities.Collector;
using ServiceEntities.UserData;
using ServiceHost;
using WebRole.Filters;
using WebRole.Models;

namespace WebRole.Controllers
{
    //[InitializeSimpleMembership]
    [BasicAuth]
    public class CollectorController : ApiController
    {
        /// <summary>
        /// CurrentUser is the replacement for ApiController.User for the CollectorController
        /// </summary>
        public IPrincipal CurrentUser { get; set; }

        public CollectorController() : base()
        {
            // initialize the CurrentUser "intrinsic"
            CurrentUser = User;
        }

        //private RecordRepository userDataRepository = new RecordRepository();
        private CollectorContext _repository;

        public CollectorContext Repository
        {
            get
            {
                if (_repository == null)
                    _repository = new CollectorContext(CurrentUser);
                return _repository;
            }
        }

        // POST colapi/collector
        public JToken Post(HttpRequestMessage req, JToken value)
        {
            //BUGBUG - need to do something about CSRF 

            //BUGBUG - clean up userDataRepository collectorContext? apparently no need to with MongoRepository
            //req.RegisterForDispose(userDataRepository);

            var list = new List<SiteLookupRecord>();
            var array = value as JArray;
            if (array != null)
            {
                // extract each object out of the array and create a SiteLookupRecord for each
                foreach (JObject r in array)
                {
                    int duration = r["Duration"] != null ? (int)r["Duration"] : 0;
                    list.Add(new SiteLookupRecord()
                    {
                        HostMacAddress = (string)r["HostMacAddress"],
                        HostIpAddress = (string)r["HostIpAddress"],
                        HostName = (string)r["HostName"],
                        WebsiteName = (string)r["WebsiteName"],
                        Timestamp = ((DateTime)r["Timestamp"]).ToString("s"),
                        Duration = duration,
                        UserId = Repository.UserId,
                        State = RecordState.New
                    });
                }

                // add all records at once
                Repository.AddRecords(list);

                TraceLog.TraceInfo(string.Format("Added {0} records for user {1}", list.Count, Repository.UserId));
            }

            return (int)ControlMessage.Normal;
        }

#if DEBUG
        // GET colapi/collector/5
        public int Get(int id)
        {
            // THIS API IS FOR TESTING ONLY - it is for creating synthetic records

            var userName = CurrentUser.Identity.Name;
            using (var userDataRepository = new UserDataRepository(userName))
            {
                var device = userDataRepository.Devices.FirstOrDefault() ?? Device.CreateNewDevice(userName);
                var collectorContext = Storage.CollectorContextFor(userName);
                var list = collectorContext.MockRecords(device, id);
                collectorContext.AddRecords(list);
                TraceLog.TraceInfo(string.Format("Added {0} records for user {1}", list.Count, Repository.UserId));
                return id;
            }
        }

        // GET colapi/collector
        public int Get()
        {
            // THIS API IS FOR TESTING ONLY - it is for creating synthetic records  
          
            // create 100 new records
            return Get(100);
        }
#endif

        public enum ControlMessage
        {
            Normal = 0,
            SuspendCollection = 1,
            DisableDevice = 2
        }
    }
}