using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace WebTimer.Client
{
    public class UpdateClient
    {
        static bool loopFlag = false;
        static bool updatingFlag = false;
        static bool checkingFlag = false;
        static bool started = false;
        static object startLock = new object();
        static Version currentVersion = null;

#if DEBUG  // 1 minute update intervals, download and version URL's in staging container
        const int timeout = 60 * 1000; // 1 minute
        const string downloadUrl = "https://webtimer.blob.core.windows.net/staging/WebTimer.msi";
        const string versionUrl = "https://webtimer.blob.core.windows.net/staging/version.txt";
#else
        const int timeout = 10 * 60 * 1000; // 10 minutes
        const string downloadUrl = "https://webtimer.blob.core.windows.net/download/WebTimer.msi";
        const string versionUrl = "https://webtimer.blob.core.windows.net/download/version.txt";
#endif
        static string appSettingsDownloadUrl = null;
        static string appSettingsVersionUrl = null;

        public static Version CurrentVersion
        {
            get
            {
                if (currentVersion == null)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                    currentVersion = new Version(fvi.FileVersion);
                }
                return currentVersion;
            }
        }

        public static string LogFilename
        {
            get
            {
                // trace filename format: "setup-2013-10-27-23-12-45.log";
                DateTime now = DateTime.UtcNow;
                string basename = "setup";
                var logFilename = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    Path.Combine(
                        @"logs",
                        String.Format("{0}-{1}.log", basename, now.ToString("yyyy-MM-dd-HH-mm-ss"))));
                return logFilename;
            }
        }

        private static string DownloadUrl
        {
            get
            {
#if DEBUG       // debug builds hardcode the staging address
                appSettingsDownloadUrl = downloadUrl;
#endif
                if (appSettingsDownloadUrl == null)
                    appSettingsDownloadUrl = ConfigurationManager.AppSettings["DownloadUrl"] ?? downloadUrl;
                return appSettingsDownloadUrl;
            }
        }

        private static string VersionUrl
        {
            get
            {
#if DEBUG       // debug builds hardcode the staging address
                appSettingsVersionUrl = versionUrl;
#endif
                if (appSettingsVersionUrl == null)
                    appSettingsVersionUrl = ConfigurationManager.AppSettings["VersionUrl"] ?? versionUrl;
                return appSettingsVersionUrl;
            }
        }

        /// <summary>
        /// Start the upload client
        /// </summary>
        public static void Start()
        {
            lock (startLock)
            {
                if (!started)
                {
                    TraceLog.TraceInfo("Starting updater");
                    ThreadStart ts = new ThreadStart(GetVersionLoop);
                    Thread thread = new Thread(ts);
                    loopFlag = true;
                    thread.Start();
                    started = true;
                }
            }
        }

        /// <summary>
        /// stop the upload client
        /// </summary>
        public static void Stop()
        {
            lock (startLock)
            {
                TraceLog.TraceInfo("Stopping updater");

                // terminate the loop
                loopFlag = false;

                TraceLog.TraceInfo("Stopped updater");
            }
        }

        #region Helpers

        private static void GetVersionLoop()
        {
            while (loopFlag)
            {
                // sleep for timeout
                Thread.Sleep(timeout);

                // don't want two concurrent updates
                if (checkingFlag || updatingFlag)
                    continue;

                try
                {
                    // locking is overkill here because only one thread that wakes up every <timeout>
                    checkingFlag = true;

                    WebServiceHelper.GetCurrentSoftwareVersion(
                        VersionUrl,
                        new WebServiceHelper.AccountDelegate((version) =>
                        {
                            if (string.IsNullOrEmpty(version))
                            {
                                TraceLog.TraceError("Could not download version file");
                                checkingFlag = false;
                                return;
                            }
                            try
                            {
                                // compare versions
                                var newVersion = new Version(version);
                                if (newVersion > CurrentVersion)
                                {
                                    TraceLog.TraceInfo(string.Format(
                                        "New version {0} more recent than current version {1}; downloading new version",
                                        newVersion, CurrentVersion));
                                    updatingFlag = true;

                                    // download and install new software
                                    WebServiceHelper.DownloadCurrentSoftware(
                                        DownloadUrl,
                                        new WebServiceHelper.AccountDelegate((filename) =>
                                        {
                                            // install the new software
                                            TraceLog.TraceInfo(string.Format("Download successful; installing {0}", filename));
                                            Install(filename);
                                        }),
                                        new WebServiceHelper.NetOpDelegate((inProgress, status) =>
                                        {
                                            // cleanup the checking flag based on network operation status (typically failures)
                                            if (inProgress == false)
                                                updatingFlag = false;
                                        }));
                                }
                                checkingFlag = false;
                            }
                            catch (Exception ex)
                            {
                                TraceLog.TraceException("Version check failed", ex);
                                checkingFlag = false;
                                updatingFlag = false;
                            }
                        }),
                        new WebServiceHelper.NetOpDelegate((inProgress, status) => 
                        {
                            // cleanup the checking flag based on network operation status (typically failures)
                            if (inProgress == false)
                                checkingFlag = false;
                        }));
                }
                catch (Exception ex)
                {
                    TraceLog.TraceException("GetVersionLoop failed", ex);
                    checkingFlag = false;
                    updatingFlag = false;
                }                
            }
        }

        private static void Install(string filename)
        {
            // spawn a new process process - we must do this outside of the service because the 
            // installer will shut down the current running service as part of the uninstall
            using (var process = new Process())
            {
                process.StartInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "msiexec.exe");
                process.StartInfo.Arguments = string.Format(
                    "/i {0} /qn /quiet /L*vx \"{1}\" REBOOT=ReallySuppress",
                    filename,
                    LogFilename);
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;

                TraceLog.TraceInfo(string.Format("Starting msiexec {0}", process.StartInfo.Arguments));
                process.Start();
            }

            // note - if the update does not succeed, the updater service will continue running and updateFlag will still be true.
            // this is a FEATURE since the update failed.  The next time the update will be downloaded will be when the service restarts,
            // not in the next update interval.  this in turn will reduce the likelihood that a bad update will create lots of junk 1MB images
            // on the user's HDD.
        }

        #endregion
    }
}
