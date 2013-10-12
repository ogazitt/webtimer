using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebRole.Helpers;

namespace WebRole.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string returnUrl)
        {
            // if this is a mobile client, redirect to the mobile sign-in page
            if (BrowserAgent.IsMobile(Request.UserAgent))
                return RedirectToAction("MobileIndex", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public ActionResult MobileIndex(string returnUrl)
        {
            /*
            // if this is a non-mobile client, redirect to the standard landing page
            if (!BrowserAgent.IsMobile(Request.UserAgent))
                return RedirectToAction("Index", "Home");
            */

            ViewBag.ReturnUrl = returnUrl;
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