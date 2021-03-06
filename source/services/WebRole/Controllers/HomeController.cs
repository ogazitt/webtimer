﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebTimer.WebRole.Helpers;
using WebTimer.WebRole.Models;

namespace WebTimer.WebRole.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string returnUrl)
        {
            // redirect production to https endpoint
            var url = Request.Url;
            if (url.Scheme == "http" && url.Host == "www.webtimer.co")
                return Redirect("https://www.webtimer.co");

            ViewBag.ReturnUrl = returnUrl;
            if (User.Identity.IsAuthenticated)
            {
                using (var context = new UsersContext())
                {
                    var user = context.UserProfiles.FirstOrDefault(u => u.UserName == User.Identity.Name);
                    if (user != null)
                    {
                        ViewBag.UserName = user.Name;
                        ViewBag.IsAdmin = user.IsAdmin;
                    }
                    else
                    {
                        ViewBag.UserName = null;
                        ViewBag.IsAdmin = false;
                    }
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

        public ActionResult Privacy()
        {
            ViewBag.Message = "Privacy Policy";
            return View();
        }

        public ActionResult Pricing()
        {
            ViewBag.Message = "Pricing";
            return View();
        }

        public ActionResult Upgrade()
        {
            ViewBag.Message = "Upgrade";
            return View("Pricing");
        }

        public ActionResult Download()
        {
            ViewBag.Message = "Download";
            return View();
        }
    }
}