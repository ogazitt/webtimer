using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

using Collector;

namespace DnsCapture
{
    class Program
    {
        static void Main(string[] args)
        {
            TraceLog.TraceDestination = TraceLog.Destination.Console;
            CollectorClient.Start();
            UploadClient.Start();

            Console.WriteLine("Press any key to stop collection");
            Console.ReadLine();

            CollectorClient.Stop();
            UploadClient.Stop();

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
