using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Helpers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
// See http://aspnet13.orcsweb.com/web-api/overview/security/preventing-cross-site-request-forgery-(csrf)-attacks
namespace WebRole.Filters
{
    public class BasicAuthAttribute : System.Web.Http.AuthorizeAttribute
    {
        const string authorizationHeader = "Authorization";
        const string authRequestHeader = "Cookie";

        public class BasicAuthCredentials
        {
            public Guid ID { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            var request = actionContext.Request;

            if (IsWebTimerRequest(request))
            {
                var creds = GetCredsFromHeader(request);
                if (creds != null)
                {
                    if (WebSecurity.Login(creds.Name, creds.Password, persistCookie: true))
                    {
                        FormsAuthentication.SetAuthCookie(creds.Name, true);
                        return;
                    }
                }
            }
            base.HandleUnauthorizedRequest(actionContext);
        }

        private bool IsWebTimerRequest(HttpRequestMessage request)
        {
            IEnumerable<string> xRequestedWithHeaders;
            if (request.Headers.TryGetValues("X-Requested-With", out xRequestedWithHeaders))
            {
                string headerValue = xRequestedWithHeaders.FirstOrDefault();
                if (!String.IsNullOrEmpty(headerValue))
                {
                    return String.Equals(headerValue, "WebTimer-Windows", StringComparison.OrdinalIgnoreCase);
                }
            }

            return false;
        }

        private BasicAuthCredentials GetCredsFromHeader(HttpRequestMessage request)
        {
            IEnumerable<string> header = new List<string>();
            if (request.Headers.TryGetValues(authorizationHeader, out header))
            {   // process basic authorization header
                string[] headerParts = header.ToArray<string>()[0].Split(' ');
                if (headerParts.Length > 1 && headerParts[0].Equals("Basic", StringComparison.OrdinalIgnoreCase))
                {
                    string credentials = Encoding.UTF8.GetString(Convert.FromBase64String(headerParts[1]));
                    int firstColonIndex = credentials.IndexOf(':');
                    string username = credentials.Substring(0, firstColonIndex);
                    string password = credentials.Substring(firstColonIndex + 1);
                    return new BasicAuthCredentials() { Name = username.ToLower(), Password = password };
                }
            }
            return null;
        }
    }
}