using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using Collector;

namespace CaptureService
{
    public partial class CaptureService : ServiceBase
    {
        public CaptureService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            CollectorClient.Start();
            UploadClient.Start();
        }

        protected override void OnStop()
        {
            CollectorClient.Stop();
            UploadClient.Stop();
        }
    }
}
