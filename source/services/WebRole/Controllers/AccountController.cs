using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using WebRole.Filters;
using WebRole.Models;
using ServiceHost;

namespace WebRole.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        //
        // POST: /Account/JsonLogin

        [AllowAnonymous]
        [HttpPost]
        public JsonResult JsonLogin(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    TraceLog.TraceInfo(string.Format("Authorized user {0}", model.UserName));
                    return Json(new { success = true, redirect = returnUrl });
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    TraceLog.TraceInfo(string.Format("Authorization failure for user {0}", model.UserName));
                }
            }

            // If we got this far, something failed
            return Json(new { errors = GetErrorsFromModelState() });
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/JsonRegister
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult JsonRegister(RegisterModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(
                        model.UserName,
                        model.Password,
                        propertyValues: new
                        {
                            Name = model.Name,
                        },
                        requireConfirmationToken: false);
                    WebSecurity.Login(model.UserName, model.Password);
                    TraceLog.TraceInfo(string.Format("Created user {0}", model.UserName));

                    using (var repository = new UserDataRepository(model.UserName))
                    {
                        repository.InitializeNewUserAccount();
                    }

                    var sentMail = EmailProcessor.SendWelcomeEmail(model.UserName);
                    if (sentMail)
                        TraceLog.TraceInfo("Sent welcome email to user " + model.UserName);

                    FormsAuthentication.SetAuthCookie(model.UserName, createPersistentCookie: false);
                    return Json(new { success = true, redirect = returnUrl });
                }
                catch (MembershipCreateUserException e)
                {
                    var error = ErrorCodeToString(e.StatusCode);
                    ModelState.AddModelError("", error);
                    TraceLog.TraceException(string.Format("Account creation failed: {0}", error), e);
                }
                catch (Exception ex)
                {
                    TraceLog.TraceException("Account creation failed", ex);
                }
            }

            // If we got this far, something failed
            return Json(new { errors = GetErrorsFromModelState() });
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        TraceLog.TraceInfo(string.Format("Dissociating {0} account for user {1}", provider, User.Identity.Name));
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }


        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message = null, string anchor = null)
        {
            ViewBag.StatusMessage = SuccessCodeToString(message);
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            ViewBag.Anchor = anchor;

            // get the appropriate user name
            using (var context = new UsersContext())
            {
                var user = context.UserProfiles.FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user != null)
                    ViewBag.UserName = user.Name;
                else
                    ViewBag.UserName = null;
            }
            return View();
        }

        //
        // POST: /Account/ManagePassword

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManagePassword(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                        TraceLog.TraceInfo(string.Format("Changed password for user {0}", User.Identity.Name));
                    }
                    catch (Exception ex)
                    {
                        changePasswordSucceeded = false;
                        TraceLog.TraceException(string.Format("Failed to change password for user {0}", User.Identity.Name), ex);
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess, Anchor = "password" });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                        TraceLog.TraceError(string.Format("Failed to change password for user {0}", User.Identity.Name));
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    TraceLog.TraceError(string.Format("Failed to change password for user {0}; password is invalid", User.Identity.Name));
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        TraceLog.TraceInfo(string.Format("Added local password for user {0}", User.Identity.Name));
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        //
        // POST: /Account/ManagePersonalInfo

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManagePersonalInfo(PersonalInfoModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (ModelState.IsValid)
            {
                using (var context = new UsersContext())
                {
                    var userProfile = context.UserProfiles.FirstOrDefault(u => u.UserName == User.Identity.Name);
                    if (userProfile != null)
                    {
                        try
                        {
                            userProfile.Name = model.Name;
                            userProfile.UserName = model.Email;
                            userProfile.Phone = model.Phone;
                            context.SaveChanges();
                            TraceLog.TraceInfo(string.Format("Changed personal information for user {0}", User.Identity.Name));
                        }
                        catch (Exception ex)
                        {
                            TraceLog.TraceException("Could not save personal information", ex);
                            return View(model);
                        }
                    }
                    else
                    {
                        TraceLog.TraceError(string.Format("Could not find user {0} to change personal information", User.Identity.Name));
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePersonalInfoFailure });
                    }
                }
                return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePersonalInfoSuccess });
            }
            else
                ModelState.AddModelError("", "One of the values is invalid.");

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                TraceLog.TraceInfo(string.Format("Added social login for user {0}", User.Identity.Name));
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        using (var repository = new UserDataRepository(model.UserName))
                        {
                            repository.InitializeNewUserAccount();
                        }

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);
                        TraceLog.TraceInfo(string.Format("Created account for user {0} using social provider {1}", model.UserName, provider));

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "Unfortunately this email has already been registered.  Please delete the existing account or try a different address.");
                        TraceLog.TraceError(string.Format("Failed to create account for user {0} using social provider {1}: email already registered", model.UserName, provider));
                        ViewBag.RegistrationFailure = true;
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        /* the following methods are used by the page to get HTML partials at runtime */

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ChangePersonalInfo(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var userInfo = new PersonalInfoModel();
            using (var context = new UsersContext())
            {
                var profile = context.UserProfiles.FirstOrDefault(u => u.UserName == User.Identity.Name);
                userInfo.Name = profile.Name;
                userInfo.Email = profile.UserName;
                userInfo.Phone = profile.Phone;
            }
            return PartialView("_ChangePersonalInfoPartial", userInfo);
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ChangePassword()
        {
            return PartialView("_ChangePasswordPartial");
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult SetPassword(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_SetPasswordPartial");
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        //
        // POST: /Account/Delete

        public ActionResult Delete()
        {
            var account = User.Identity.Name;
            WebSecurity.Logout();
            ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(account);
            ((SimpleMembershipProvider)Membership.Provider).DeleteUser(account, true);
            using (var repository = new UserDataRepository(User))
            {
                repository.RemoveUserData();
            }
            TraceLog.TraceInfo(string.Format("Deleted account for user {0}", account));

            return RedirectToAction("Index", "Home");
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            ChangePersonalInfoSuccess,
            ChangePersonalInfoFailure,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        private static string SuccessCodeToString(ManageMessageId? messageId)
        {
            if (!messageId.HasValue)
                return "";
            switch (messageId.Value)
            {
                case ManageMessageId.ChangePasswordSuccess:
                    return "Your password has been changed.";
                case ManageMessageId.SetPasswordSuccess:
                    return "Your password has been set.";
                case ManageMessageId.RemoveLoginSuccess:
                    return "The social login was removed.";
                case ManageMessageId.ChangePersonalInfoSuccess:
                    return "Your personal information has been changed.";
                case ManageMessageId.ChangePersonalInfoFailure:
                    return "Your personal information could not be saved.  Please contact support@webtimer.co.";
                default:
                    return "";
            }
        }
        #endregion
    }
}