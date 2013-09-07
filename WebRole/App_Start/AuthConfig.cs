using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using WebRole.Models;

namespace WebRole
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            OAuthWebSecurity.RegisterTwitterClient(
                consumerKey: "358635997554397",
                consumerSecret: "f1e505d41b79380fe9da309f8ce3b984");

            OAuthWebSecurity.RegisterFacebookClient(
                appId: "358635997554397",
                appSecret: "f1e505d41b79380fe9da309f8ce3b984");

            OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
