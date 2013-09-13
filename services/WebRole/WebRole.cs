using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceHost;

namespace WebRole
{
    public class WebRoleEntryPoint : Microsoft.WindowsAzure.ServiceRuntime.RoleEntryPoint
    {
        public override bool OnStart()
        {
            // initialize azure logging
            if (HostEnvironment.IsAzureLoggingEnabled)
                TraceLog.InitializeAzureLogging();

            if (HostEnvironment.IsSplunkLoggingEnabled)
                TraceLog.InitializeSplunkLogging();

            // Log function entrance (must do this after DiagnosticsMonitor has been initialized)
            TraceLog.TraceFunction();
            TraceLog.TraceInfo("WebRole started");

            /*
            // the database must exist for the role to run
            if (!UserDataContext.InitializeDatabase())
            {
                TraceLog.TraceFatal("Cannot initialize the UserData database");
                return false;
            }
             * */

            return base.OnStart(); 
        }
    }
}