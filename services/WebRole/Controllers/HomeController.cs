using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebRole.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string returnUrl)
        {
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