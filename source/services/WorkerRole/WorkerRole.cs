using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using WebTimer.ServiceHost;

namespace WebTimer.WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        const int timeout = 30000;  // 30 seconds

        public static string Me
        {
            get { return String.Concat(Environment.MachineName.ToLower(), "-", Thread.CurrentThread.ManagedThreadId.ToString()); }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            // initialize azure logging
            if (HostEnvironment.IsAzureLoggingEnabled)
                TraceLog.InitializeAzureLogging();

            if (HostEnvironment.IsSplunkLoggingEnabled)
                TraceLog.InitializeSplunkLogging();

            // Log function entrance (must do this after DiagnosticsMonitor has been initialized)
            TraceLog.TraceFunction();

            return base.OnStart();
        }

        public override void Run()
        {
            // initialize splunk logging (we do this here instead of OnStart so that the Azure logger has time to really start)
            TraceLog.TraceInfo("WorkerRole started");

            // the database must exist for the role to run
            if (!UserDataContext.InitializeDatabase())
            {
                TraceLog.TraceFatal("Cannot initialize the UserData database");
                return;
            }

#if DISABLE
            // check the database schema versions to make sure there is no version mismatch
            if (!Storage.NewUserContext.CheckSchemaVersion())
            {
                TraceLog.TraceFatal("User database schema is out of sync, unrecoverable error");
                return;
            }
            if (!Storage.NewSuggestionsContext.CheckSchemaVersion())
            {
                TraceLog.TraceFatal("Suggestions database schema is out of sync, unrecoverable error");
                return;
            }

            // (re)create the database constants if the code contains a newer version
            if (!Storage.NewUserContext.VersionConstants(Me))
            {
                TraceLog.TraceFatal("Cannot check and/or update the User database constants, unrecoverable error");
                return;
            }
            if (!Storage.NewSuggestionsContext.VersionConstants(Me))
            {
                TraceLog.TraceFatal("Cannot check and/or update the Suggestions database constants, unrecoverable error");
                return;
            }
#endif

            // get the number of workers (default is 0)
#if false           
            int workflowWorkerCount = ConfigurationSettings.GetAsNullableInt(HostEnvironment.WorkflowWorkerCountConfigKey) ?? 0;
            var workflowWorkerArray = new WorkflowWorker.WorkflowWorker[workflowWorkerCount];
            int timerWorkerCount = ConfigurationSettings.GetAsNullableInt(HostEnvironment.TimerWorkerCountConfigKey) ?? 0;
            var timerWorkerArray = new TimerWorker.TimerWorker[timerWorkerCount];
            int mailWorkerCount = ConfigurationSettings.GetAsNullableInt(HostEnvironment.MailWorkerCountConfigKey) ?? 0;
            var mailWorkerArray = new MailWorker.MailWorker[mailWorkerCount];
#endif
            int analyticsWorkerCount = ConfigurationSettings.GetAsNullableInt(HostEnvironment.AnalyticsWorkerCountConfigKey) ?? 0;
            var analyticsWorkerArray = new WebTimer.AnalyticsWorker.AnalyticsWorker[analyticsWorkerCount];
            int processorWorkerCount = ConfigurationSettings.GetAsNullableInt(HostEnvironment.ProcessorWorkerCountConfigKey) ?? 0;
            var processorWorkerArray = new WebTimer.ProcessorWorker.ProcessorWorker[processorWorkerCount];

            // run an infinite loop doing the following:
            //   check whether the worker services have stopped (indicated by a null reference)
            //   (re)start the service on a new thread if necessary
            //   sleep for the timeout period
            while (true)
            {
#if false
                // start workflow and timer workers in both dev and deployed Azure fabric
                RestartWorkerThreads<WorkflowWorker.WorkflowWorker>(workflowWorkerArray);
                RestartWorkerThreads<TimerWorker.TimerWorker>(timerWorkerArray);

                // start mail and speech workers only in deployed Azure fabric
                if (!HostEnvironment.IsAzureDevFabric)
                    RestartWorkerThreads<MailWorker.MailWorker>(mailWorkerArray);
                if (!HostEnvironment.IsAzureDevFabric)
                    RestartWorkerThreads<SpeechWorker.SpeechWorker>(speechWorkerArray);
#endif
                // start Analytics worker in both dev and deployed Azure fabric
                RestartWorkerThreads<WebTimer.AnalyticsWorker.AnalyticsWorker>(analyticsWorkerArray);

                // start Processor worker in both dev and deployed Azure fabric
                RestartWorkerThreads<WebTimer.ProcessorWorker.ProcessorWorker>(processorWorkerArray);

                // sleep for the timeout period
                Thread.Sleep(timeout);
            }
        }

        #region Helpers

        void RestartWorkerThreads<T>(Array array) where T : IWorker, new() 
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array.GetValue(i) == null)
                {
                    int threadNum = i;
                    Thread thread = new Thread(() =>
                    {
                        try
                        {
                            T worker = new T();
                            array.SetValue(worker, threadNum);

                            // sleep for a fraction of the worker's Timeout that corresponds to the position in the array
                            // this is to spread out the workers relatively evenly across the entire Timeout interval
                            Thread.Sleep(worker.Timeout * threadNum / array.Length);
                            worker.Start();
                        }
                        catch (Exception ex)
                        {
                            TraceLog.TraceException(String.Format("Exception caught in {0}{1}", typeof(T).Name, threadNum.ToString()), ex);
                            TraceLog.TraceFatal(String.Format("{0}{1} died and will be recycled", typeof(T).Name, threadNum.ToString()));
                            array.SetValue(null, threadNum);
                        }
                    }) { Name = typeof(T).Name + i.ToString() };

                    thread.Start();
                    TraceLog.TraceInfo(thread.Name + " started");
                }
            }
        }

        #endregion Helpers
    }
}
