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

        private UserDataRepository _userDataRepository;
        private CollectorContext _collectorRepository;

        public UserDataRepository UserDataRepository
        {
            get
            {
                if (_userDataRepository == null)
                    _userDataRepository = new UserDataRepository(CurrentUser);
                return _userDataRepository;
            }
        }

        public CollectorContext CollectorRepository
        {
            get
            {
                if (_collectorRepository == null)
                    _collectorRepository = new CollectorContext(CurrentUser);
                return _collectorRepository;
            }
        }

        // POST colapi/collector
        public ServiceResponse Post(HttpRequestMessage req, JToken value)
        {
            //BUGBUG - need to do something about CSRF 

            try
            {
                var controlMessage = ControlMessage.Normal;

                var obj = value as JObject;
                var record = new CollectorRecord()
                {
                    DeviceId = (string)obj[CollectorFields.DeviceId],
                    DeviceName = (string)obj[CollectorFields.DeviceName],
                    UserId = CollectorRepository.UserId,
                    State = RecordState.New,
                    RecordList = new List<SiteLookupRecord>()
                };

                // reject messages from a null device ID or UserID
                if (string.IsNullOrEmpty(record.DeviceId) || string.IsNullOrEmpty(record.UserId))
                {
                    TraceLog.TraceError(string.Format("Rejecting invalid record: {0}", obj.ToString()));
                    return new ServiceResponse() { ControlMessage = controlMessage, RecordsProcessed = -1 };
                }

                // store device software version and timestamp (if this isn't a new device)
                var device = UserDataRepository.Devices.FirstOrDefault(d => d.DeviceId == record.DeviceId);
                if (device != null)
                {
                    // this device is marked for deletion
                    if (device.Enabled == null)
                    {
                        TraceLog.TraceInfo(string.Format("Deleting suspended device {0} for user {1}", device.Name, device.UserId));
                        UserDataRepository.DeleteDevice(device);
                        return new ServiceResponse() { ControlMessage = ControlMessage.DisableDevice, RecordsProcessed = 0 };
                    }

                    // tell the client to suspend collection if the device was disabled by the user
                    if (!device.Enabled.Value)
                        controlMessage = ControlMessage.SuspendCollection;

                    if (obj[CollectorFields.SoftwareVersion] != null)
                        device.SoftwareVersion = (string)obj[CollectorFields.SoftwareVersion];
                    device.Timestamp = DateTime.UtcNow;
                    UserDataRepository.SaveChanges();
                }

                var array = obj[CollectorFields.Records] as JArray;
                if (array != null && array.Count > 0)
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
                    CollectorRepository.AddRecord(record);

                    TraceLog.TraceInfo(string.Format("Added {0} records for user {1}", record.RecordList.Count, CollectorRepository.UserId));
                }

                var response = new ServiceResponse()
                {
                    RecordsProcessed = array != null ? array.Count : 0,
                    ControlMessage = controlMessage
                };

                return response;
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("Collector Post failed", ex);
                throw;
            }
        }
    }
}