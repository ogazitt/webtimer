namespace WebRole.Controllers
{
    using System.Linq;
    using System.Web.Http;
    using Breeze.WebApi;
    using Newtonsoft.Json.Linq;
    using Filters;
    using Models;
    using ServiceEntities.UserData;
    using System.Collections.Generic;
    using ServiceHost;

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

        // GET ~/api/Dashboard/WebSessions
        [HttpGet]
        public IQueryable<WebSession> WebSessions(string start, string end)
        {
            return _repository.WebSessions
                .Where(s => s.Start.CompareTo(start) > 0 && s.Start.CompareTo(end) < 0);
        }

        // GET ~/api/Dashboard/ConsolidatedWebSessions
        [HttpGet]
        public IQueryable<WebSession> ConsolidatedWebSessions(string start, string end)
        {
            return _repository.WebSessions
                .Where(s => s.Start.CompareTo(start) > 0 && s.Start.CompareTo(end) < 0)
                .GroupBy(s => s.Site)
                .Select(sg => new WebSession()
                {
                    Site = sg.Key,
                    Duration = sg.Sum(d => d.Duration),
                    Category = sg.First().Category,
                });
        }

        // GET ~/api/Dashboard/ConsolidatedWebSessions
        [HttpGet]
        public IQueryable<HighChartSeries> CategoryTotals(string start, string end)
        {
            // group the sessions by person, then by category, and return a HighChartSeries per person
            //   with each series containing an array of HighChartResults which have the name of the category and the sum 
            //   of the duration of all the websessions for that category for that person
            /*
            return _repository.WebSessions
                .Where(s => s.Start.CompareTo(start) > 0 && s.Start.CompareTo(end) < 0 && s.Category != null)
                .GroupBy(s => s.Device.Person)
                .Select(sp => new HighChartSeries()
                {
                    Name = sp.Key.Name,
                    Data = sp.GroupBy(p => p.Category).Select(hcr => new HighChartResult()
                    {
                        Name = hcr.Key,
                        Y = hcr.Sum(d => d.Duration)
                    })
                });
            */
            var categories = _repository.Categories.Select(c => c.Name).ToList();
            return _repository.WebSessions
                .Where(s => s.Start.CompareTo(start) > 0 && s.Start.CompareTo(end) < 0 && s.Category != null)
                .GroupBy(s => s.Device.Person)
                .Select(sp => new HighChartSeries()
                {
                    Name = sp.Key.Name,
                    Data = from cat in categories
                           join sess in sp on cat equals sess.Category into g
                           select new HighChartResult()
                           {
                               Name = cat,
                               Y = g.Count() > 0 ? g.Sum(d => d.Duration) : 0
                           }
                });
            /*
            return _repository.WebSessions
                .Where(s => s.Start.CompareTo(start) > 0 && s.Start.CompareTo(end) < 0 && s.Category != null)
                .GroupBy(s => s.Device.Person)
                .Select(sp => new HighChartSeries()
                {
                    Name = sp.Key.Name,
                    Data = from cat in _repository.Categories join sess in sp on cat.Name equals sess.Category into g
                           select new HighChartResult()
                    {
                        Name = cat.Name,
                        Y = g.Sum(d => d.Duration)
                    }
                });
             */
            /*
                .GroupBy(s => s.Category)
                .Select(sg => new HighChartResult()
                {
                    Name = sg.Key,
                    Y = sg.Sum(d => d.Duration)
                });
             */
        }

        /*
        // GET ~/api/Dashboard/Dash
        [HttpGet]
        public IQueryable<WebSession> Dash(int personId, string start, string end)
        {
            return _repository.WebSessions
                .Where(s => s.Device.PersonId == personId && 
                            s.Start.CompareTo(start) > 0 && 
                            s.Start.CompareTo(end) < 0)
                .GroupBy(s => s.Category)
                .Select(sg => new 
                {
                    Category = sg.Key,
                    Sum = sg.Sum(d => d.Duration)
                });
        }
         */

    }
}