using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using MongoDB.Driver;
using MongoRepository;

using ServiceEntities.Collector;
using ServiceEntities.SiteMap;
using ServiceEntities.UserData;
using System.Text.RegularExpressions;

namespace ServiceEntities.SiteMap
{
    public class SiteMapRepository 
    {
        IRepository<SiteMapping> _mappingRepository;
        IRepository<SiteExpression> _expressionRepository;
        IRepository<UnknownSite> _unknownSiteRepository;
        List<Regex> regexList = new List<Regex>();
        Dictionary<Regex, SiteExpression> dictionary = new Dictionary<Regex, SiteExpression>();

        public SiteMapRepository(IRepository<SiteMapping> mappingRepository, IRepository<SiteExpression> expressionRepository, IRepository<UnknownSite> unknownSiteRepository)
        {
            _mappingRepository = mappingRepository;
            _expressionRepository = expressionRepository;
            _unknownSiteRepository = unknownSiteRepository;
        }

        public void Initialize()
        {
            InitializeDB();
            PopulateSiteExpressions();
        }

        /// <summary>
        /// Add a sitemap to the database
        /// </summary>
        /// <param name="sitemap">a SiteMap</param>
        /// <returns>true for success, false for failure</returns>
        public bool AddSiteMapping(SiteMapping sitemap)
        {
            _mappingRepository.Add(sitemap);
            return true;
        }

        /// <summary>
        /// Add multiple sitemaps at the same time using the same connection
        /// </summary>
        /// <param name="sitemaps">a list of SiteMappings</param>
        /// <returns>true for success, false for failure</returns>
        public bool AddSiteMappings(List<SiteMapping> sitemaps)
        {
            _mappingRepository.Add(sitemaps);
            return true;
        }

        /// <summary>
        /// Get the site mapping for this site
        /// </summary>
        /// <returns></returns>
        public SiteMapping GetSiteMapping(string site)
        {
            if (site == null)
                return null;
            site = site.ToLowerInvariant();

            // find out whether the site is recognized by one of the site expressions
            var expr = RunSiteExpressions(site);
            if (expr != null)
                site = expr.Site;

            // find a mapping
            var siteMapping = _mappingRepository.FirstOrDefault(r => r.Site == site);
            if (siteMapping != null)
            {
                // if the mapping is to suppress the site, return null
                if (siteMapping.Category == Category.Categories.Suppressed)
                    return null;
                return siteMapping;
            }

            // site mapping not found - add the site as an unknown site
            var unknownSite = _unknownSiteRepository.FirstOrDefault(r => r.Site == site);
            if (unknownSite == null)
                _unknownSiteRepository.Add(new UnknownSite() { Site = site });
            return new SiteMapping() { Site = site };
        }

        #region Helpers

        private void InitializeDB()
        {
            if (_expressionRepository.Count() == 0)
                _expressionRepository.Add(SiteExpression.Expressions);

            if (_mappingRepository.Count() == 0)
                _mappingRepository.Add(SiteMapping.SiteMappings);
        }

        private void PopulateSiteExpressions()
        {
            // populate the regex list
            foreach (var expr in _expressionRepository.ToList())
            {
                var regex = new Regex(expr.Regex, RegexOptions.Compiled);
                regexList.Add(regex);
                dictionary.Add(regex, expr);
            }
        }

        private SiteExpression RunSiteExpressions(string site)
        {
            foreach (var regex in dictionary.Keys)
                if (regex.IsMatch(site))
                    return dictionary[regex];
            return null;
        }

        #endregion
    }
}