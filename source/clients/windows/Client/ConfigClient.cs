using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace WebTimer.Client
{
    public class ConfigClient
    {
        public const string Credentials = "Credentials";
        public const string DeviceId = "DeviceId";
        public const string DeviceName = "DeviceName";
        public const string Disabled = "Disabled";
        public const string Email = "Email";
        public const string Name = "Name";
        public const string Version = "Version";

        private static Dictionary<string, string> dict;
        private const string configFilename = @"webtimer.config";

        private static Dictionary<string, string> Dict
        {
            get
            {
                if (dict == null)
                {
                    using (var fs = File.Open(configFilename, FileMode.OpenOrCreate))
                    using (var reader = new StreamReader(fs))
                    {
                        dict = JsonConvert.DeserializeObject<Dictionary<string,string>>(reader.ReadToEnd());
                    }
                    if (dict == null)
                        dict = new Dictionary<string, string>();
                }
                return dict;
            }
        }

        public static void Clear()
        {
            // reset the dictionary and the config file
            dict = new Dictionary<string, string>();
            WriteFile();
        }

        public static string Read(string key, bool refresh = false)
        {
            try
            {
                // if refresh flag is set, re-read the config from the file
                if (refresh)
                    dict = null;

                string result = null;
                Dict.TryGetValue(key, out result);
                return result;
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("Could not read key " + key, ex);
                return null;
            }
        }

        public static void Write(string key, string value)
        {
            try
            {
                Dict[key] = value;
                WriteFile();
            }
            catch (Exception ex)
            {
                TraceLog.TraceException(string.Format("Cloud not write key {0} value {1}", key, value), ex);
            }
        }

        private static void WriteFile()
        {
            try
            {
                using (var fs = File.Open(configFilename, FileMode.Truncate))
                using (var writer = new StreamWriter(fs))
                {
                    writer.Write(JsonConvert.SerializeObject(Dict, Formatting.Indented));
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("Cloud not write config file", ex);
            }
        }
    }
}
