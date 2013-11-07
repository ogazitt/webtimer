using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTimer.DeployClient
{
    class Program
    {
        const string ProductionConnectionString = "ProductionConnectionString";
        const string Dev1ConnectionString = "Dev1ConnectionString";
        const string Dev1BSConnectionString = "Dev1BSConnectionString";
        const string DownloadContainer = "DownloadContainer";
        const string StagingContainer = "StagingContainer";

        static string[] Files = { "WebTimerSetup.exe", "WebTimer.msi", "version.txt" };
        static string[] ContentTypes = { "application/x-msdownload", "application/octet-stream", "text/plain" };

        /// <summary>
        /// Usage: DeployClient.exe [/p[rod]] [/directory pathname]
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var connectionString = Dev1ConnectionString;
            var container = StagingContainer;
            var directory = Directory.GetCurrentDirectory();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "/h":
                    case "/?":
                        Usage();
                        return;
                    case "/d":
                    case "/download":
                        container = DownloadContainer;
                        break;
                    case "/s":
                    case "/staging":
                        container = StagingContainer;
                        break;
                    case "/directory":
                        directory = args[++i];
                        break;
                    case "/dev1":
                        connectionString = Dev1ConnectionString;
                        break;
                    case "/dev1bs":
                        connectionString = Dev1BSConnectionString;
                        break;
                    case "/production":
                        connectionString = ProductionConnectionString;
                        break;
                }
            }

            Console.WriteLine();
            Console.WriteLine("====================");
            Console.WriteLine("DeployClient Utility");
            Console.WriteLine("====================");
            Console.WriteLine();
            Console.WriteLine(string.Format("Uploading into {0}/{1} from directory {2}", connectionString, container, directory));

            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                // Provide the configSetter with the initial value
                configSetter(ConfigurationManager.AppSettings[configName]);
            });

            var account = CloudStorageAccount.FromConfigurationSetting(connectionString);
            var cloudBlobClient = account.CreateCloudBlobClient();
            var containerName = ConfigurationManager.AppSettings[container];

            var cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
            cloudBlobContainer.CreateIfNotExist();

            Console.WriteLine("Terminate the upload by pressing <enter>");
            Console.WriteLine();

            // start uploading the first file (note Files are serialized)
            Upload(cloudBlobContainer, directory, 0);

            Console.ReadLine();
        }

        private static void Usage()
        {
            Console.WriteLine("Usage: DeployClient.exe [/p[rod]] [/directory <pathname>]"); 
        }

        /// <summary>
        /// This method chains each of the uploads to the successful completion of the previous one.
        /// This is because the version.txt file should not be updated until and unless the other uploads succeed.
        /// </summary>
        /// <param name="cloudBlobContainer"></param>
        /// <param name="directory"></param>
        /// <param name="index"></param>
        private static void Upload(CloudBlobContainer cloudBlobContainer, string directory, int index)
        {
            // check for last file
            if (index >= Files.Length)
                return;

            var filename = Files[index];
            try
            {
                var blob = cloudBlobContainer.GetBlobReference(filename);
                var path = Path.Combine(directory, filename);
                var stream = File.Open(path, FileMode.Open, FileAccess.Read);

                Console.WriteLine("--------------------------------------------------------------------------------");
                Console.WriteLine("Uploading " + filename);
                var iar = blob.BeginUploadFromStream(
                    stream,
                    new AsyncCallback((asyncResult) =>
                    {
                        string uri = (string)asyncResult.AsyncState;
                        try
                        {
                            stream.Close();
                            blob.Properties.ContentType = ContentTypes[index];
                            blob.SetProperties();
                            Console.WriteLine("Successfully uploaded " + uri);
                            Console.WriteLine();
                            index++;
                            Upload(cloudBlobContainer, directory, index);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("Finishing the upload of {0} failed with {1}", uri, ex.Message));
                        }
                    }),
                    blob.Uri.ToString());
                blob.EndUploadFromStream(iar);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Uploading {0} failed with {1}", filename, ex.Message));
            }

        }
    }
}
