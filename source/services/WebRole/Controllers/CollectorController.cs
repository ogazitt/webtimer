using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http;
using Newtonsoft.Json.Linq;

using WebTimer.ServiceEntities.Collector;
using WebTimer.ServiceEntities.UserData;
using WebTimer.ServiceHost;
using WebTimer.WebRole.Filters;
using WebTimer.WebRole.Models;

namespace WebTimer.WebRole.Controllers
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
        public ServiceResponse Post(HttpRequestMessage req, JToken value)
        {
            //BUGBUG - need to do something about CSRF 

            var obj = value as JObject;
            var record = new CollectorRecord()
            {
                DeviceId = (string) obj[CollectorFields.DeviceId],
                DeviceName = (string)obj[CollectorFields.DeviceName],
                UserId = Repository.UserId,
                State = RecordState.New,
                RecordList = new List<SiteLookupRecord>()
            };

            var array = obj[CollectorFields.Records] as JArray;
            if (array != null)
            {
                // extract each object out of the array and create a SiteLookupRecord for each
                foreach (JObject r in array)
                {
                    int duration = r["Duration"] != null ? (int)r["Duration"] : 0;
                    record.RecordList.Add(new SiteLookupRecord()
                    {
                        WebsiteName = (string)r["WebsiteName"],
                        Timestamp = ((DateTime)r["Timestamp"]).ToString("s"),
                        Duration = duration,
                    });
                }

                // add all records at once
                Repository.AddRecord(record);

                TraceLog.TraceInfo(string.Format("Added {0} records for user {1}", record.RecordList.Count, Repository.UserId));
            }

            var response = new ServiceResponse()
            {
                RecordsProcessed = array != null ? array.Count : 0,
                ControlMessage = ControlMessage.Normal
            };

            return response;
        }
    }
}