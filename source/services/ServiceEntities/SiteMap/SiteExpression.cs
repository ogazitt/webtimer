using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MongoRepository;

namespace WebTimer.ServiceEntities.SiteMap
{
    [CollectionName("SiteExpressions")]
    public class SiteExpression : Entity
    {
        public string Regex { get; set; }
        public string Site { get; set; }

        public static List<SiteExpression> Expressions = new List<SiteExpression>() {
            // facebook
            new SiteExpression() { Regex = @"^fbcdn-.*\.akamaihd\.net$", Site = "facebook.com" },
            new SiteExpression() { Regex = @"^fbstatic-.*\.akamaihd\.net$", Site = "facebook.com" },
            new SiteExpression() { Regex = @"^fbexternal-.*\.akamaihd\.net$", Site = "facebook.com" },
            new SiteExpression() { Regex = @".*\.facebook\.com$", Site = "facebook.com" },
            new SiteExpression() { Regex = @".*\.fbcdn\.net$", Site = "facebook.com" },
            // tumblr
            new SiteExpression() { Regex = @".*\.tumblr\.com$", Site = "tumblr.com" },
            // twitter
            new SiteExpression() { Regex = @".*\.twitter\.com$", Site = "twitter.com" },
            // wordpress
            new SiteExpression() { Regex = @".*\.wordpress\.com$", Site = "wordpress.com" },
            // pinterest
            new SiteExpression() { Regex = @".*\.pinterest\.com$", Site = "pinterest.com" },
            // netflix
            new SiteExpression() { Regex = @".*\.netflix\.com$", Site = "netflix.com" },
            // hulu
            new SiteExpression() { Regex = @".*\.hulu\.com$", Site = "hulu.com" },
            new SiteExpression() { Regex = @".*\.huluim\.com$", Site = "hulu.com" },
            // youtube
            new SiteExpression() { Regex = @".*\.youtube\.com$", Site = "youtube.com" },
            new SiteExpression() { Regex = @".*\.ytimg\.com$", Site = "youtube.com" },
            // yahoo mail
            new SiteExpression() { Regex = @".*\.mail\.yahoo\.com$", Site = "mail.yahoo.com" },
            // gmail
            new SiteExpression() { Regex = @"^mail\.google\.com$", Site = "mail.google.com" },
            // hotmail
            new SiteExpression() { Regex = @"^mail\.live\.com$", Site = "mail.live.com" },
            new SiteExpression() { Regex = @"^Scalendar\.live\.com$", Site = "calendar.live.com" },
            // msn
            new SiteExpression() { Regex = @".*\.msn\.com$", Site = "msn.com" },
            // yahoo
            new SiteExpression() { Regex = @".*\.yahoo\.com$", Site = "www.yahoo.com" },
            new SiteExpression() { Regex = @".*\.yimg\.com$", Site = "www.yahoo.com" },
            // amazon
            new SiteExpression() { Regex = @".*\.amazon\.com$", Site = "amazon.com" },
            new SiteExpression() { Regex = @"^www\.amazon\.*", Site = "amazon.com" },
            // ebay
            new SiteExpression() { Regex = @".*\.ebay\.com$", Site = "ebay.com" },
            // noise
            new SiteExpression() { Regex = @".*\.gstatic\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @"^maps\.amung\.us$", Site = "suppress" },
            new SiteExpression() { Regex = @".*\.doubleclick\.net$", Site = "suppress" },
            new SiteExpression() { Regex = @".*\.clicktale\.net$", Site = "suppress" },
            new SiteExpression() { Regex = @".*\.google-analytics\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @".*\.imrworldwide\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @".*\.agkn\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @"^safebrowsing.*\.google\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @".*\.googletagservices\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @".*\.googleadservices\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @".*\.googlesyndication\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @".*\.adzerk\.net$", Site = "suppress" },
            new SiteExpression() { Regex = @".*\.googleuserconsent\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @"^apis.googleapis\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @"^ajax.googleapis\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @"^connect.facebook\.net$", Site = "suppress" },
            new SiteExpression() { Regex = @"^tinyurl\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @".*\.clickwall\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @".*\.2mdn\.net$", Site = "suppress" },
            new SiteExpression() { Regex = @".*\.dropbox\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @"^teredo\.ipv6\.microsoft\.com$", Site = "suppress" },
            new SiteExpression() { Regex = @"^webtimer.*\.windows\.net$", Site = "suppress" },
            new SiteExpression() { Regex = @"^webtimer.*\.cloudapp\.net$", Site = "suppress" },
        };
    }
}