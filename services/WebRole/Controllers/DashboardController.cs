namespace WebRole.Controllers
{
    using System;
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
            try
            {
                return _repository.Metadata();
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("Metadata query failed", ex);
                throw;
            }
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

        // GET ~/api/Dashboard/WebSessionTimeline
        [HttpGet]
        public IQueryable<HighChartSeries> WebSessionTimeline(string start, string end, int personId, string category = null)
        {
            // time axis labels
            var times = new string[] 
            { 
                "12am", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11",
                "12pm", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12am"
            };
            var activity = new int[times.Length];

            var sessions = _repository.WebSessions
                .Where(s => s.Start.CompareTo(start) > 0 && s.Start.CompareTo(end) < 0 &&
                            s.Device.PersonId == personId &&
                            s.Duration > 10 && // session must be longer than 10 seconds
                            category == null ? s.Category != null : s.Category == category)
                .ToList();
                        
            // construct a list of times the user has been active
            foreach (var session in sessions)
            {
                var startDT = Convert.ToDateTime(session.Start);
                var startHour = startDT.Hour;
                var endDT = startDT.AddSeconds(session.Duration);
                var endHour = endDT.Hour > startHour ? endDT.Hour : startHour + 1;  // make the record show at least an hour
                for (int i = startHour; i <= endHour; i++)
                    activity[i] = 1;
            }

            // construct the result list in the timeline
            var timeline = new List<HighChartResult>();
            for (int i = 0; i < times.Length; i++)
            {
                timeline.Add(new HighChartResult()
                {
                    Name = times[i],
                    Y = activity[i] > 0 ? activity[i] : (decimal?) null,
                });
            }

            // construct the wrapper series
            var result = new List<HighChartSeries>()
            {
                new HighChartSeries()
                {
                    Data = timeline,
                    Color = "blue"
                }
            };

            return result.AsQueryable();
        }

        // GET ~/api/Dashboard/ConsolidatedWebSessions
        [HttpGet]
        public IQueryable<HighChartSeries> ConsolidatedWebSessions(string start, string end, int personId, string category)
        {
            var results = _repository.WebSessions
                .Where(s => s.Start.CompareTo(start) > 0 && s.Start.CompareTo(end) < 0 && s.Category == category && 
                            s.Device.PersonId == personId)
                .GroupBy(s => s.Site)
                .Select(hcr => new HighChartResult()
                {
                    Name = hcr.Key,
                    Y = (int?)hcr.Sum(d => d.Duration) ?? 0
                }).ToList();

            // postprocess
            var postResults = new List<HighChartResult>();
            int i = 1;
            foreach (var result in results)
            {                
                // Y (minutes) is returned as a decimal which has a scale of 1 (xxx.x)
                result.Y = decimal.Truncate(result.Y.Value * 10 / 60) / 10;
                result.Color = Colors.List[i % Colors.List.Count()];
                if (result.Y > 0)
                {
                    postResults.Add(result);
                    i++;
                }
            }

            // create a series corresponding to this category
            var cat = _repository.Categories.FirstOrDefault(c => c.Name == category);
            var series = new List<HighChartSeries>()
            {
                new HighChartSeries() 
                {
                    Name = category,
                    Color = cat != null ? cat.Color : null,
                    Data = postResults,
                }
            };
            
            return series.AsQueryable<HighChartSeries>();
        }

        // GET ~/api/Dashboard/CategoryTotals
        [HttpGet]
        public IQueryable<HighChartSeries> CategoryTotals(string start, string end, int? personId = null)
        {
            // prepare the category list for joining and post-processing
            var categories = _repository.Categories.ToList();
            var categoryNames = categories.Select(c => c.Name).ToList();
            var categoryColors = new Dictionary<string, string>();

            // prepare the categories hash
            if (personId.HasValue)
            {
                foreach (var cat in categories)
                    categoryColors[cat.Name] = cat.Color;
            }

            // group the sessions by person, then by category, and return a HighChartSeries per person
            //   with each series containing an array of HighChartResults which have the name of the category and the sum 
            //   of the duration of all the websessions for that category for that person
            var results = _repository.WebSessions
                .Where(s => s.Start.CompareTo(start) > 0 && s.Start.CompareTo(end) < 0 && s.Category != null &&
                            (personId.HasValue ? s.Device.PersonId == personId : true))
                .GroupBy(s => s.Device.Person)
                .Select(sp => new HighChartSeries()
                {
                    Name = sp.Key.Name,
                    Color = sp.Key.Color,
                    Data = from cat in categoryNames
                           join sess in sp on cat equals sess.Category into g
                           select new HighChartResult()
                           {
                               Name = cat,
                               Y = (int?)g.Sum(d => d.Duration) ?? 0
                           }
                }).ToList();

            // postprocess
            foreach (var series in results)
            {
                foreach (var result in series.Data)
                {
                    // add a color if this is the only series (for a single person)
                    if (personId.HasValue)
                        result.Color = categoryColors[result.Name];
                    // Y (minutes) is returned as a decimal which has a scale of 1 (xxx.x)
                    result.Y = decimal.Truncate(result.Y.Value * 10 / 60) / 10;
                }
            }

            return results.AsQueryable<HighChartSeries>();
        }
    }
}