using System;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using FluentAssertions;
using Processor;
using ServiceEntities.Collector;
using ServiceEntities.SiteMap;
using Shared = Tests.Shared;

namespace CollectorWorker.Tests
{
    /// <summary>
    /// Summary description for CollectorTest
    /// </summary>
    [SetUpFixture]
    public class TestBase
    {
        protected List<string> macAddresses = new List<string>();
        protected List<string> websiteNames = new List<string>();
        protected Dictionary<string, string> ipAddresses = new Dictionary<string, string>();
        protected SiteMapRepository siteMapRepository = new SiteMapRepository(
            new Shared.Mocks.MockMongoRepositority<SiteMapping>(),
            new Shared.Mocks.MockMongoRepositority<SiteExpression>(),
            new Shared.Mocks.MockMongoRepositority<UnknownSite>(),
            new Shared.Mocks.MockMongoRepositority<SiteMapVersion>());

        const int macAddressCount = 10;
        const int websiteCount = 10;

        const string hexDigit = "0123456789ABCDEF";
        const string siteName = "abcdefghijklmnopqrstuvwxyz";

        private Random rand = new Random(DateTime.Now.Millisecond);
        private bool initialized = false;
        private object setupLock = new object();

        [SetUp]
        public void TextFixtureSetup()
        {
            if (!initialized)
                lock (setupLock)
                {
                    GenerateMacAddresses();
                    GenerateWebsites();
                    GenerateIpAddresses();
                    initialized = true;
                }
        }

        private void GenerateWebsites()
        {
            for (int i = 0; i < websiteCount; i++)
                websiteNames.Add(CreateWebsiteName());
            websiteNames.Count.Should().Be(websiteCount);
        }

        private void GenerateMacAddresses()
        {
            for (int i = 0; i < macAddressCount; i++)
                macAddresses.Add(CreateMacAddress());
            macAddresses.Count.Should().Be(macAddressCount);
        }

        private void GenerateIpAddresses()
        {
            for (int i = 0; i < macAddressCount; i++)
                ipAddresses[macAddresses[i]] = CreateIpAddress();
            ipAddresses.Values.Count.Should().Be(macAddressCount);
        }

        private string CreateWebsiteName()
        {
            websiteCount.Should().BeLessOrEqualTo(siteName.Length);
            return "www." + siteName.Substring(rand.Next(websiteCount), 1) + ".com";
        }

        private string CreateMacAddress()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 12; i++)
                sb.Append(hexDigit.Substring(rand.Next(hexDigit.Length), 1));
            var addr = sb.ToString();
            addr.Length.Should().Be(12);
            return addr;
        }

        private string CreateIpAddress()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                sb.Append(String.Format("{0}", rand.Next(256)));
                if (i < 3)
                    sb.Append('.');
            }
            var addr = sb.ToString();
            return addr;
        }
    }
}
