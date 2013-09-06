using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using WebRole.Collector.Models;

namespace WebCollector.Controllers
{
    public class CollectorController : ApiController
    {
        private RecordRepository repository = new RecordRepository();

        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public JToken Post(HttpRequestMessage req, JToken value)
        {
            req.RegisterForDispose(repository);
            var array = value as JArray;
            if (array != null)
            {
                foreach (JObject r in array)
                    repository.CreateRecord(new SiteLookupRecord() 
                    { 
                        HostMacAddress = (string) r["Host"], 
                        Sitename = (string) r["Destination"], 
                        Timestamp = (string) r["Timestamp"] 
                    });
            }
             
            return value;
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}