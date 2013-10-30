using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using WebTimer.Client;

namespace WebTimer.CaptureService
{
    public partial class CaptureService : ServiceBase
    {
        public CaptureService()
        {
            InitializeComponent();
            CanHandlePowerEvent = true;
            CanStop = true;
            CanShutdown = true;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                // set the current directory to the service directory (default is SYSTEM32)
                System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory); 

                // set the trace destination
                TraceLog.TraceDestination = TraceLog.Destination.File;
                TraceFile.TraceName = @"CaptureService";

                TraceLog.TraceInfo("Starting Capture Service");
                CollectorClient.Start();
                UploadClient.Start();
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("OnStart: Caught exception, Start aborted", ex);
                throw ex;
            }
        }

        protected override void OnStop()
        {
            try
            {
                TraceLog.TraceInfo("Stopping Capture Service");
                CollectorClient.Stop();
                UploadClient.Stop();
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("OnStop: Caught exception", ex);
                throw ex;
            }
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            TraceLog.TraceInfo("Power event: " + powerStatus.ToString());
            switch (powerStatus)
            {
                case PowerBroadcastStatus.ResumeAutomatic:
                case PowerBroadcastStatus.ResumeCritical:
                case PowerBroadcastStatus.ResumeSuspend:
                    try
                    {
                        TraceLog.TraceInfo("Resuming: power state " + powerStatus.ToString());
                        CollectorClient.Start();
                        UploadClient.Start();
                    }
                    catch (Exception ex)
                    {
                        TraceLog.TraceException("OnPowerEvent: Caught exception, Resume processing aborted", ex);                        
                        throw;
                    }
                    break;
            }
            return base.OnPowerEvent(powerStatus);
        }
    }
}
