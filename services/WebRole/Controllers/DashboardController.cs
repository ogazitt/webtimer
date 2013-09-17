namespace WebRole.Controllers
{
    using System.Linq;
    using System.Web.Http;
    using Breeze.WebApi;
    using Newtonsoft.Json.Linq;
    using Filters;
    using Models;
    using ServiceEntities.UserData;

    [Authorize]
    [BreezeController]
    public class DashboardController : ApiController
    {
        private readonly DashboardRepository _repository;

        public DashboardController()
        {
            _repository = new DashboardRepository(User);
        }

        // GET ~/api/Dashboard/Metadata 
        [HttpGet]
        public string Metadata()
        {
            return _repository.Metadata();
        }

        // POST ~/api/Dashboard/SaveChanges
        [HttpPost]
        [ValidateHttpAntiForgeryToken]
        public SaveResult SaveChanges(JObject saveBundle)
        {
            return _repository.SaveChanges(saveBundle);
        }

        // GET ~/api/Dashboard/Devices
        [HttpGet]
        public IQueryable<Device> Devices()
        {
            return _repository.Devices;
        }

        // GET ~/api/Dashboard/People
        [HttpGet]
        public IQueryable<Person> People()
        {
            return _repository.People;
        }

        // GET ~/api/Dashboard/People
        [HttpGet]
        public IQueryable<WebSession> WebSessions()
        {
            return _repository.WebSessions;
        }
    }
}