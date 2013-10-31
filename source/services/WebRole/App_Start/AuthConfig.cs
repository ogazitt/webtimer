using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;

using WebTimer.ServiceHost;
using WebTimer.WebRole.Models;

namespace WebTimer.WebRole
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            RegisterSimpleMembership();

            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            /*
            OAuthWebSecurity.RegisterTwitterClient(
                consumerKey: "358635997554397",
                consumerSecret: "f1e505d41b79380fe9da309f8ce3b984");
            */
            OAuthWebSecurity.RegisterFacebookClient(
                appId: "307251696084143",
                appSecret: "0fa520aac1c6d4d1c5f71a00ea5c6438");
            
            OAuthWebSecurity.RegisterGoogleClient();
        }

        public static void RegisterSimpleMembership()
        {
            Database.SetInitializer<UsersContext>(null);

            try
            {
                using (var context = new UsersContext())
                {
                    if (!context.Database.Exists())
                    {
                        // Create the SimpleMembership database without Entity Framework migration schema
                        ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                        TraceLog.TraceInfo("Created membership database");
                    }
                }

                WebSecurity.InitializeDatabaseConnection(HostEnvironment.UserProfileConnection, "UserProfile", "UserId", "UserName", autoCreateTables: true);
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("Could not initialize Simple Membership database", ex);
                throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588", ex);
            }
        }
    }
}
