using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ServiceEntities.UserData;
using ServiceHost;

namespace WebRole.Controllers
{
    public class OldDashboardController : Controller
    {
        private UserDataContext db = Storage.NewUserDataContext;

        //
        // GET: /Dashboard/

        public ActionResult Index()
        {
            var websessions = db.WebSessions.Where(s => s.UserId == User.Identity.Name).Include(w => w.Device);
            return View(websessions.ToList());
        }

        public ActionResult Raw()
        {
            var websessions = db.WebSessions.Where(s => s.UserId == User.Identity.Name).Include(w => w.Device);
            return View(websessions.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}