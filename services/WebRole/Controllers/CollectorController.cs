using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using ServiceHost;
using ServiceEntities.Collector;
using WebRole.Filters;

namespace WebCollector.Controllers
{
    [BasicAuth]
    public class CollectorController : ApiController
    {
        //private RecordRepository repository = new RecordRepository();
        private CollectorContext repository;

        public CollectorController()
        {
            repository = new CollectorContext(User);
        }

        // POST colapi/collector
        public JToken Post(HttpRequestMessage req, JToken value)
        {
            //BUGBUG - clean up repository context?
            //req.RegisterForDispose(repository);
            var array = value as JArray;
            if (array != null)
            {
                foreach (JObject r in array)
                {
                    /*
                    repository.CreateRecord(new SiteLookupRecord()
                    {
                        HostMacAddress = (string)r["Host"],
                        Sitename = (string)r["Destination"],
                        Timestamp = (string)r["Timestamp"]
                    });
                     */
                    repository.Add(new SiteLookupRecord()
                    {
                        HostMacAddress = (string)r["HostMacAddress"],
                        HostIpAddress = (string)r["HostIpAddress"],
                        HostName = (string)r["HostName"],
                        WebsiteName = (string)r["WebsiteName"],
                        Timestamp = (string)r["Timestamp"]
                    });
                }
            }
             
            return value;
        }

#if KILL
        // GET colapi/collector
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET colapi/collector/5
        public string Get(int id)
        {
            return "value";
        }

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