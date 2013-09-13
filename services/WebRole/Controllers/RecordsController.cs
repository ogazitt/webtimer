using ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebRole.Controllers
{
    public class RecordsController : Controller //, IDisposable
    {
        private CollectorContext repository = Storage.NewCollectorContext;

        //
        // GET: /Records/

        public ActionResult Index()
        {
            return View(repository.GetAllRecords());
        }


        /*
        private RecordRepository repository = new RecordRepository();
        private bool disposed = false;

        //
        // GET: /Records/

        public ActionResult Index()
        {
            return View(repository.GetAllRecords());
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
                    this.repository.Dispose();
                }
            }

            this.disposed = true;
        }

        # endregion
         */
    }
}
