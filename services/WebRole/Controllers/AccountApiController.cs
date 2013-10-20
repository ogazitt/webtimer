using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using WebMatrix.WebData;
using WebRole.Helpers;
using WebRole.Models;
using Newtonsoft.Json.Linq;

using ServiceHost;

namespace WebRole.Controllers
{
    public class AccountApiController : ApiController
    {
        //
        // POST: /AccountApi/Register
        [HttpPost]
        [AllowAnonymous]
        public string Register(LoginModel model)
        {
            // BUGBUG: access to this API must be controlled - e.g. the client must send a secret
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                    WebSecurity.Login(model.UserName, model.Password);
                    TraceLog.TraceInfo(string.Format("Created user {0}", model.UserName));

                    using (var repository = new UserDataRepository(model.UserName))
                    {
                        repository.InitializeNewUserAccount();
                    }

                    EmailProcessor.SendWelcomeEmail(model.UserName);

                    FormsAuthentication.SetAuthCookie(model.UserName, createPersistentCookie: false);
                    return model.UserName;
                }
                catch (MembershipCreateUserException ex)
                {
                    TraceLog.TraceException(string.Format("Could not create user {0}", model.UserName), ex);
                    return "Error: " + ex.Message;
                }
            }

            // If we got this far, something failed
            return null;
        }
        
        //
        // POST: /AccountApi/ValidateUser
        [HttpPost]
        [AllowAnonymous]
        public string ValidateUser(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to validate the user
                try
                {
                    if (WebSecurity.Login(model.UserName, model.Password))
                        return model.UserName;
                }
                catch (Exception ex)
                {
                    TraceLog.TraceException(string.Format("Login failed for user {0}", model.UserName), ex);
                }
            }

            // If we got this far, something failed
            return null;
        }
    }
}