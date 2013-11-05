using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

using WebTimer.ServiceHost;
using WebTimer.WebRole.Helpers;
using WebTimer.WebRole.Models;

namespace WebTimer.WebRole.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index(AdminMessageId? message = null, string anchor = null)
        {
            ViewBag.StatusMessage = SuccessCodeToString(message);
            ViewBag.ReturnUrl = Url.Action("Index");
            ViewBag.Anchor = anchor;

            if (User.Identity.IsAuthenticated)
            {
                using (var context = new UsersContext())
                {
                    // ensure this is an admin
                    var user = context.UserProfiles.FirstOrDefault(u => u.UserName == User.Identity.Name);
                    if (user == null || user.IsAdmin == false)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    // get the appropriate user name
                    ViewBag.UserName = user.Name;
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        //
        // POST: /Account/Impersonate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Impersonate(ImpersonateModel model)
        {
            ViewBag.ReturnUrl = Url.Action("Index");
            if (ModelState.IsValid)
            {
                using (var context = new UsersContext())
                {
                    var userProfile = context.UserProfiles.FirstOrDefault(u => u.UserName == User.Identity.Name);
                    if (userProfile != null && userProfile.IsAdmin)
                    {
                        try
                        {
                            var userToImpersonate = context.UserProfiles.FirstOrDefault(u => u.UserName == model.UserName);
                            if (userToImpersonate.PermissionToImpersonate)
                            {
                                TraceLog.TraceInfo(string.Format("Admin {0} impersonating user {1}", userProfile.UserName, userToImpersonate.UserName));
                                // set auth cookie and redirect
                                FormsAuthentication.SetAuthCookie(model.UserName, false);
                                //HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
/*
                                if (authCookie != null)
                                {
                                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                                    // set up a surrogate IPrincipal
                                    var principal = new RolePrincipal(new FormsIdentity(authTicket));
                                    User = principal;
                                    return principal;
                                }
 */
                                return Redirect("/");
                            }
                            else
                            {
                                TraceLog.TraceError(string.Format("User {0} did not grant permission to impersonate", userToImpersonate.UserName));
                                return RedirectToAction("Index", new { Message = AdminMessageId.PermissionToImpersonateFailure });
                            }
                        }
                        catch (Exception ex)
                        {
                            TraceLog.TraceException("Could not impersonate user", ex);
                            return RedirectToAction("Index", new { Message = AdminMessageId.ImpersonateFailure });
                        }
                    }
                    else
                    {
                        TraceLog.TraceError(string.Format("User {0} not found or is not admin", User.Identity.Name));
                        return RedirectToAction("Index", new { Message = AdminMessageId.NotAdmin });
                    }
                }
            }
            else
                ModelState.AddModelError("", "One of the values is invalid.");

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /* the following methods are used by the page to get HTML partials at runtime */

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult Impersonate(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var userInfo = new ImpersonateModel();
            return PartialView("_ImpersonatePartial", userInfo);
        }

        public enum AdminMessageId
        {
            ImpersonateFailure,
            NotAdmin,
            PermissionToImpersonateFailure,
        }

        private static string SuccessCodeToString(AdminMessageId? messageId)
        {
            if (!messageId.HasValue)
                return "";
            switch (messageId.Value)
            {
                case AdminMessageId.ImpersonateFailure:
                    return "User could not be impersonated.";
                case AdminMessageId.NotAdmin:
                    return "Logged in user is not an admin.";
                case AdminMessageId.PermissionToImpersonateFailure:
                    return "User did not grant permission to be impersonated.";
                default:
                    return "";
            }
        }
    }
}