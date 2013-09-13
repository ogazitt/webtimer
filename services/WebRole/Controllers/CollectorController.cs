using System.Collections.Generic;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using ServiceEntities.Collector;
using ServiceHost;
using WebRole.Filters;

namespace WebRole.Controllers
{
    [BasicAuth]
    public class CollectorController : ApiController
    {
        /// <summary>
        /// CurrentUser is the replacement for ApiController.User for the CollectorController
        /// </summary>
        public IPrincipal CurrentUser { get; set; }

        public CollectorController()
        {
            // initialize the CurrentUser "intrinsic"
            CurrentUser = User;
        }

        //private RecordRepository repository = new RecordRepository();
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

            //BUGBUG - clean up repository context? apparently no need to with MongoRepository
            //req.RegisterForDispose(repository);

            var list = new List<SiteLookupRecord>();
            var array = value as JArray;
            if (array != null)
            {
                // extract each object out of the array and create a SiteLookupRecord for each
                foreach (JObject r in array)
                {
                    list.Add(new SiteLookupRecord()
                    {
                        HostMacAddress = (string)r["HostMacAddress"],
                        HostIpAddress = (string)r["HostIpAddress"],
                        HostName = (string)r["HostName"],
                        WebsiteName = (string)r["WebsiteName"],
                        Timestamp = (string)r["Timestamp"],
                        UserId = Repository.UserId,
                        State = RecordState.New
                    });
                }

                // add all records at once
                Repository.AddRecords(list);

                TraceLog.TraceInfo(string.Format("Added {0} records for user {1}", list.Count, Repository.UserId));
            }

            return list.Count;
        }

#if DEBUG
        // GET colapi/collector/5
        [AllowAnonymous]
        public int Get(int count)
        {
            // THIS API IS FOR TESTING ONLY - it is for creating synthetic records

            var context = new CollectorContext("dummyuser");
            var list = context.MockRecords(count);
            context.AddRecords(list);
            TraceLog.TraceInfo(string.Format("Added {0} records for user {1}", list.Count, Repository.UserId));
            return count;
        }

        // GET colapi/collector
        [AllowAnonymous]
        public int Get()
        {
            // THIS API IS FOR TESTING ONLY - it is for creating synthetic records  
          
            // create 100 new records
            return Get(100);
        }
#endif

#if KILL
        // PUT colapi/collector/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE colapi/collector/5
        public void Delete(int id)
        {
        }
#endif
    }
}