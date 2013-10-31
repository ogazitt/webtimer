using WebTimer.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Security;
using WebMatrix.WebData;

namespace WebTimer.WebRole.Helpers
{
    public class AuthHelper
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

        public static IPrincipal Authenticate(HttpActionContext actionContext)
        {
            var request = actionContext.Request;
            var userName = VerifyLogin(request);
            if (userName != null)
            {
                FormsAuthentication.SetAuthCookie(userName, true);
                HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie != null)
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                    // set up a surrogate IPrincipal
                    var principal = new RolePrincipal(new FormsIdentity(authTicket));
                    HttpContext.Current.User = principal;
                    return principal;
                }
            }

            return null;
        }

        public static string VerifyLogin(HttpRequestMessage request)
        {
            if (IsWebTimerRequest(request))
            {
                var creds = GetCredsFromHeader(request);
                if (creds != null)
                {
                    if (WebSecurity.Login(creds.Name, creds.Password, persistCookie: true))
                    {
                        return creds.Name;
                    }
                    TraceLog.TraceError(string.Format("Collector API login failed for {0}", creds.Name));
                }
                TraceLog.TraceError("Collector API authorization error: could not extract credentials from HTTP headers");
            }
            return null;
        }

        private static bool IsWebTimerRequest(HttpRequestMessage request)
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

        private static BasicAuthCredentials GetCredsFromHeader(HttpRequestMessage request)
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