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
        static List<Record> records = new List<Record>();
        static bool uploadFlag = false;

        static void Main(string[] args)
        {
            TraceHelper.TraceDestination = TraceHelper.Destination.Console;
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
