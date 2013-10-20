using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebRole.Helpers;
using WebRole.Models;

namespace WebRole.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (User.Identity.IsAuthenticated)
            {
                using (var context = new UsersContext())
                {
                    var user = context.UserProfiles.FirstOrDefault(u => u.UserName == User.Identity.Name);
                    if (user != null)
                        ViewBag.UserName = user.Name;
                    else
                        ViewBag.UserName = null;
                }
            }
            return View();
        }
      
        public ActionResult About()
        {
            ViewBag.Message = "About WebTimer";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact";
            return View();
        }

        public ActionResult Terms()
        {
            ViewBag.Message = "Terms of Service";
            return View();
        }

        public ActionResult Download()
        {
            ViewBag.Message = "Download";
            return View();
        }
    }
}