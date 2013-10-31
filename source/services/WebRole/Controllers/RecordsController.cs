using WebTimer.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebTimer.WebRole.Controllers
{
    [Authorize]
    public class RecordsController : Controller //, IDisposable
    {
        private CollectorContext _repository;

        public CollectorContext Repository
        {
            get
            {
                if (_repository == null)
                    _repository = new CollectorContext(User.Identity.Name);
                return _repository;
            }
        }

        //
        // GET: /Records/

        public ActionResult Index()
        {
            return View(Repository.GetAllRecords());
        }


        /*
        private RecordRepository userDataRepository = new RecordRepository();
        private bool disposed = false;

        //
        // GET: /Records/

        public ActionResult Index()
        {
            return View(userDataRepository.GetAllRecords());
        }

        # region IDisposable

        public new void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected new virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.userDataRepository.Dispose();
                }
            }

            this.disposed = true;
        }

        # endregion
         */
    }
}
