using System;
using NUnit.Framework;
using FluentAssertions;
using WebTimer.ServiceEntities.SiteMap;
using WebTimer.ServiceEntities.UserData;
using Shared = WebTimer.Tests.Shared;

namespace WebTimer.ServiceEntities.Tests
{
    [TestFixture]
    public class SiteMappingTests
    {
        SiteMapRepository siteMapRepository;

        [SetUp]
        public void SetUp()
        {
            siteMapRepository = siteMapRepository ?? new SiteMapRepository(
                new Shared.Mocks.MockMongoRepositority<SiteMapping>(),
                new Shared.Mocks.MockMongoRepositority<SiteExpression>(),
                new Shared.Mocks.MockMongoRepositority<UnknownSite>(),
                new Shared.Mocks.MockMongoRepositority<SiteMapVersion>());
        }

        [Test]
        public void AddSiteMap()
        {
            siteMapRepository.AddSiteMapping(new SiteMapping() { Site = "foo", Category = "bar" });
            var map = siteMapRepository.GetSiteMapping("foo");
            map.Should().NotBe(null);
            map.Site.Should().Be("foo");
        }

        [Test]
        public void AddDuplicateSiteMap()
        {
            siteMapRepository.AddSiteMapping(new SiteMapping() { Site = "foo", Category = "bar" });
            siteMapRepository.AddSiteMapping(new SiteMapping() { Site = "foo", Category = "baz" });  // note different category
            var map = siteMapRepository.GetSiteMapping("foo");
            map.Should().NotBe(null);
            map.Site.Should().Be("foo");
            map.Category.Should().Be("bar"); // not "baz"
        }

        [Test]
        public void CatchAllRule()
        {
            var map = siteMapRepository.GetSiteMapping("abcdefg.foobarbaz.com");
            map.Should().NotBe(null);
            map.Site.Should().Be("foobarbaz.com");
        }
    }
}
