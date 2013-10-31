using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using WebTimer.ServiceEntities.UserData;
using WebTimer.ServiceHost;
using WebTimer.WebRole.Models;

namespace WebTimer.WebRole.Controllers
{
    public class OldDashboardController : Controller
    {
        private UserDataRepository _repository;

        public OldDashboardController() : base()
        {
            _repository = new UserDataRepository(User);
        }

        //
        // GET: /Dashboard/

        public ActionResult Index()
        {
            var websessions = _repository.WebSessions.Include(w => w.Device);
            return View(websessions.ToList());
        }

        public ActionResult Raw()
        {
            var websessions = _repository.WebSessions.Include(w => w.Device);
            return View(websessions.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            _repository.Dispose();
            base.Dispose(disposing);
        }
    }
}