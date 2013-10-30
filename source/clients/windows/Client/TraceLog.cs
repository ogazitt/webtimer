using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WebTimer.Client
{
    public class TraceLog
    {
        public enum Destination
        {
            Console,
            File
        };

        public static Destination TraceDestination { get; set; }

        private static string session;
        public static string Session
        {
            get
            {
                if (session == null)
                {
                    var deviceName = Environment.MachineName;
                    session = String.Format("{0}:{1}", Client, DateTime.UtcNow.ToString("s"));
                }
                return session;
            }
            set
            {
                session = value;
            }
        }

        private static string client;
        public static string Client
        {
            get
            {
                if (client == null)
                {
                    var deviceName = Environment.MachineName;
                    client = deviceName;
                }
                return client;
            }
            set
            {
                client = value;
            }
        }

        public static string LevelText(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Fatal:
                    return "Fatal Error";
                case LogLevel.Error:
                    return "Error";
                case LogLevel.Info:
                    return "Information";
                case LogLevel.Detail:
                    return "Detail";
                default:
                    return "Unknown";
            }
        }

        public enum LogLevel
        {
            Fatal,
            Error,
            Info,
            Detail
        }

        public static void TraceDetail(string message)
        {
            string msg = String.Format(
                "{0}\n{1}",
                MethodInfoText(),
                message);
            TraceLine(msg, LogLevel.Detail);
        }

        public static void TraceInfo(string message)
        {
            string msg = String.Format(
                "{0}\n{1}",
                MethodInfoText(),
                message);
            TraceLine(msg, LogLevel.Info);
        }

        public static void TraceError(string message)
        {
            string msg = String.Format(
                "{0}\n{1}",
                MethodInfoText(),
                message);
            TraceLine(msg, LogLevel.Error);
        }

        public static void TraceException(string message, Exception e)
        {
            StringBuilder sb = new StringBuilder(); 
            int level = 0;
            while (e != null)
            {
                sb.Append(String.Format("[{0}] {1}\n", level++, e.Message));
                e = e.InnerException;
            }

            string msg = String.Format(
                "{0}\n{1}\nExceptions:\n{2}\nStackTrace:\n{3}",
                MethodInfoText(),
                message,
                sb.ToString(),
                StackTraceText(5));
            TraceLine(msg, LogLevel.Error);        
        }

        public static void TraceFatal(string message)
        {
            string msg = String.Format(
                "{0} ***FATAL ERROR***\n{1}",
                MethodInfoText(),
                message);
            TraceLine(msg, LogLevel.Fatal);
        }

        // do not compile this in unless this is a DEBUG build
        [Conditional("DEBUG")]
        public static void TraceFunction()
        {
            string msg = String.Format(
                "Entering {0}",
                MethodInfoText());
            TraceLine(msg, LogLevel.Detail);
        }

        public static void TraceLine(string message, LogLevel level)
        {
            TraceLine(message, LevelText(level));
        }

        public static void TraceLine(string message, string level)
        {
            message = String.Format("{0}: {1}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"), message);
            switch (TraceDestination)
            {
                case Destination.Console:
                    Console.WriteLine(String.Format("{0}:{1}", level, message));
                    break;
                case Destination.File:
                    TraceFile.WriteLine(message, level);
                    break;
            }
        }

        #region Helpers

        private static string MethodInfoText()
        {
#if DEBUG
            StackTrace st = new StackTrace(2, true);
#else
            StackTrace st = new StackTrace(1, true);
#endif
            StackFrame sf = st.GetFrame(0);
            return StackFrameText(sf);
        }

        private static string StackFrameText(StackFrame sf)
        {
            string fullFileName = sf.GetFileName();
            string filename = "UnknownFile";
            if (!string.IsNullOrEmpty(fullFileName))
            {
                string[] parts = fullFileName.Split('\\');
                filename = parts[parts.Length - 1];
            }
            string msg = String.Format(
                "{0}() in {1}:{2}",
                sf.GetMethod().Name,
                filename,
                sf.GetFileLineNumber().ToString());
            return msg;
        }

        private static string StackTraceText(int depth)
        {
#if DEBUG
            StackTrace st = new StackTrace(2, true);
#else
            StackTrace st = new StackTrace(1, true);
#endif
            StackFrame[] frames = st.GetFrames();
            StringBuilder sb = new StringBuilder();
            int i = 0;
            while (i < frames.Length)
            {
                sb.Append(String.Format("({0}) {1}\n", i, StackFrameText(frames[i])));
                if (++i >= depth) break;
            }
            return sb.ToString();
        }

        #endregion
    }

    public class TraceFile
    {
        const int MaxFileSize = 1024 * 1024; // 1MB max file size
        static object writeLock = new object();

        public static string TraceName { get; set; }

        private static string traceFilename;
        public static string TraceFilename
        {
            get
            {
                if (traceFilename == null)
                {
                    // trace filename format: "trace-2012-06-16-23-12-45-123.json";
                    DateTime now = DateTime.UtcNow;
                    string basename = TraceName ?? "trace";
                    traceFilename = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        Path.Combine(
                            @"logs",
                            String.Format("{0}-{1}.json", basename, now.ToString("yyyy-MM-dd-HH-mm-ss-fff"))));
                }
                return traceFilename;
            }
        }
        
        public static void WriteLine(string message, string level)
        {
            // create a json record
            var record = new TraceRecord()
            {
                //Client = TraceLog.Client,
                LogLevel = level, 
                Message = message,
                Session = TraceLog.Session,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") 
            };
            var json = JsonConvert.SerializeObject(record);

            lock (writeLock)
            {
                // enter a retry loop writing the record to the trace file
                int retryCount = 2;
                while (retryCount > 0)
                {
                    try
                    {
                        if (traceFilename == null)
                        {
                            // create the file
                            using (var stream = File.Create(TraceFilename))
                            using (var writer = new StreamWriter(stream))
                            {
                                // log the file creation
                                var createRecord = new TraceRecord()
                                {
                                    //Client = TraceLog.Client,
                                    LogLevel = TraceLog.LevelText(TraceLog.LogLevel.Info),
                                    Message = "Created new trace file " + TraceFilename,
                                    Session = TraceLog.Session,
                                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")
                                };
                                var createJson = JsonConvert.SerializeObject(createRecord);
                                writer.WriteLine(createJson);
                                writer.Flush();
                            }
                        }

                        // open the file
                        using (var stream = File.Open(TraceFilename, FileMode.Append, FileAccess.Write, FileShare.Read))
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.WriteLine(json);
                            writer.Flush();

                            // reset the trace filename if it exceeds the maximum file size
                            if (writer.BaseStream.Position > MaxFileSize)
                                traceFilename = null;
                        }

                        // success - terminate the enclosing retry loop
                        break;
                    }
                    catch (Exception)
                    {
                        // the file wasn't opened or written to correctly - try to start with a new file in the next iteration of the retry loop
                        traceFilename = null;
                        retryCount--;
                    }
                }
            }
        }
    }

    class TraceRecord
    {
        //public string Client { get; set; }
        public string LogLevel { get; set; }
        public string Timestamp { get; set; }
        public string Session { get; set; }
        public string Message { get; set; }
    }
}