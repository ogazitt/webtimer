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
        public IQueryable<HighChartSeries> ConsolidatedWebSessions(string start, string end, int personId, string category = null)
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
                result.Y = decimal.Truncate(result.Y * 10 / 60) / 10;
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

            if (personId.HasValue)
            {
                foreach (var cat in categories)
                    categoryColors[cat.Name] = cat.Color;
            }

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
                    result.Y = decimal.Truncate(result.Y * 10 / 60) / 10;
                }
            }

            return results.AsQueryable<HighChartSeries>();
        }

        // GET ~/api/Dashboard/CategoryTotals2
        [HttpGet]
        public IQueryable<HighChartSeries> CategoryTotals2(string start, string end)
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
            var results = _repository.WebSessions
                .Where(s => s.Start.CompareTo(start) > 0 && s.Start.CompareTo(end) < 0 && s.Category != null)
                .GroupBy(s => s.Device.Person)
                .Select(sp => new HighChartSeries()
                {
                    Name = sp.Key.Name,
                    Color = sp.Key.Color,
                    Data = from cat in categories
                           join sess in sp on cat equals sess.Category into g
                           select new HighChartResult()
                           {
                               Name = cat,
                               Y = g.Count() > 0 ? g.Sum(d => d.Duration) : 0
                           }
                })
                .ToList();

            // postprocess by converting seconds to minutes
            foreach (var series in results)
            {
                foreach (var result in series.Data)
                {
                    // Y is returned as a decimal which has a scale of 1 (xxx.x)
                    result.Y = decimal.Truncate(result.Y * 10 / 60) / 10;
                }
            }

            return results.AsQueryable<HighChartSeries>();

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

        // GET ~/api/Dashboard/CategoryTotalsForPerson
        [HttpGet]
        public IQueryable<HighChartSeries> CategoryTotalsForPerson(string start, string end, int personId)
        {
            // prepare the category list for joining and post-processing
            var categories = _repository.Categories.ToList();
            var categoryNames = categories.Select(c => c.Name).ToList();
            var categoryColors = new Dictionary<string, string>();
            foreach (var cat in categories)
                categoryColors[cat.Name] = cat.Color;

            var results = _repository.WebSessions
                .Where(s => s.Start.CompareTo(start) > 0 && s.Start.CompareTo(end) < 0 && s.Category != null &&
                            s.Device.PersonId == personId)
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

            // postprocess with colors and convert seconds to minutes
            foreach (var series in results)
            {
                foreach (var result in series.Data)
                {
                    result.Color = categoryColors[result.Name];
                    // Y is returned as a decimal which has a scale of 1 (xxx.x)
                    result.Y = decimal.Truncate(result.Y * 10 / 60) / 10;
                }
            }

            return results.AsQueryable<HighChartSeries>();
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