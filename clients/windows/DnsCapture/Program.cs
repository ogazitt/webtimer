using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using PacketDotNet;
using SharpPcap;

namespace DnsCapture
{
    class Program
    {
        static List<Record> records = new List<Record>();
        static bool uploadFlag = false;

        static void Main(string[] args)
        {
            // Print SharpPcap version
            string ver = SharpPcap.Version.VersionString;
            Console.WriteLine("DnsCapture with version {0}", ver);

            // Retrieve the device list
            var devices = CaptureDeviceList.Instance;

            // If no devices were found print an error
            if(devices.Count < 1)
            {
                Console.WriteLine("No devices were found on this machine");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("The following devices are available on this machine:");
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine();

            int i = 0;

            // Print out the devices
            foreach(var dev in devices)
            {
                Console.WriteLine("{0}) {1} {2}", i, dev.Name, dev.Description);
                i++;
            }

            Console.WriteLine();
            Console.Write("-- Please choose a device to capture: ");
            i = int.Parse( Console.ReadLine() );

            var device = devices[i];

            // Register our handler function to the 'packet arrival' event
            device.OnPacketArrival +=
                new PacketArrivalEventHandler(device_OnPacketArrival);
            
            // Open the device for capturing
            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

            Console.WriteLine();
            Console.WriteLine("-- Listening on {0} {1}, hit 'Enter' to stop...",
                device.Name, device.Description);

            /*
            RawCapture packet;

            // Capture packets using GetNextPacket()
            while( (packet = device.GetNextPacket()) != null )
            {
                // Prints the time and length of each received packet
                var time = packet.Timeval.Date;
                var len = packet.Data.Length;
                Console.WriteLine("{0}:{1}:{2},{3} Len={4}", 
                    time.Hour, time.Minute, time.Second, time.Millisecond, len);
            }
            */

            // DNS only
            string filter = "udp dst port 53";
            device.Filter = filter;

            // Start the capturing process
            device.StartCapture();

            // start the uploader thread if running in production
            ThreadStart ts = new ThreadStart(Send);
            Thread thread = new Thread(ts);
            uploadFlag = true;
            thread.Start();

            // Wait for 'Enter' from the user.
            Console.ReadLine();

            // Stop the capturing process
            device.StopCapture();

            Console.WriteLine("-- Capture stopped.");

            // Print out the device statistics
            Console.WriteLine(device.Statistics.ToString());

            // Close the pcap device
            device.Close();

            uploadFlag = false;

            Console.Write("Hit 'Enter' to exit...");
            Console.ReadLine();
        }
    
        private static void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            if(packet is PacketDotNet.EthernetPacket)
            {
                var eth = packet as EthernetPacket;

                // this syntax is from PacketDotNet 0.13.0 which isn't supported in the nuget package (0.12.0)
                //var udp = packet.Extract(typeof(UdpPacket)) as UdpPacket;
                var udp = Extract(packet, typeof(UdpPacket)) as UdpPacket;
                if (udp != null)
                {
                    // extract hostname.  it is encoded in segments: one-byte length followed by the segment in ASCII
                    // the first segment starts at 0x0c

                    const int START = 0x0c;

                    int start = START;
                    byte len = udp.PayloadData[start];
                    var sb = new StringBuilder();
                    while (len > 0)
                    {
                        sb.Append(Encoding.ASCII.GetString(udp.PayloadData, start + 1, len));
                        start += len + 1;
                        len = udp.PayloadData[start];
                        if (len > 0)
                            sb.Append('.');
                    }
                    string websiteName = sb.ToString();

                    var hostMacAddress = eth.SourceHwAddress.ToString();
                    var hostIpAddress = "";
                    var hostName = "";

                    var ip = packet.PayloadPacket as IPv4Packet;
                    if (ip != null)
                    {
                        hostIpAddress = ip.SourceAddress.ToString();
                        IPHostEntry entry = null;
                        if (Environment.Version.CompareTo(new System.Version(4, 0)) < 0)
                            entry = Dns.GetHostByAddress(ip.SourceAddress);
                        else
                            entry = Dns.GetHostEntry(ip.SourceAddress);
                        if (entry != null)
                            hostName = entry.HostName;
                    }
                    Console.WriteLine(String.Format("Website Name: {0}; this host: {1}", websiteName, hostName));
                    lock (records)
                    {
                        records.Add(new Record() 
                        { 
                            HostMacAddress = hostMacAddress, 
                            HostIpAddress = hostIpAddress,
                            HostName = hostName, 
                            WebsiteName = websiteName, 
                            Timestamp = DateTime.Now.ToString("s") 
                        });
                    }
                }
            }
        }

        private static Packet Extract(Packet p, Type type)
        {
            do
            {
                if (type.IsAssignableFrom(p.GetType()))
                {
                    return p;
                }
                p = p.PayloadPacket;
            }
            while (p != null);
            return null;
        }

        private static void Send()
        {
            while (uploadFlag)
            {
                // snap the record list
                var recordList = new List<Record>();
                lock (records)
                {
                    recordList.AddRange(records);
                    records.Clear();
                }

                var sendQueue = new List<Record>();

                // compact the record list
                var uniqueHosts = recordList.Select(r => r.HostName).Distinct();
                foreach (var host in uniqueHosts)
                {
                    var uniqueDests = recordList.Where(r => r.HostName == host).Select(r => r.WebsiteName).Distinct();
                    foreach (var dest in uniqueDests)
                    {
                        var record = recordList.FirstOrDefault(r => r.HostName == host && r.WebsiteName == dest);
                        if (record != null)
                            sendQueue.Add(record);
                    }
                }

                if (sendQueue.Count > 0)
                {
                    WebServiceHelper.PostRecords(new User { Name = "ogazitt", Password = "zrc022.." }, sendQueue, null, null);
                    // send the queue to the web service
                    /*
                    var request = WebRequest.Create(new Uri("http://localhost:3212/api/values"));
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    //request.Accept = "application/json";
                    var stream = request.GetRequestStream();
                    var json = JsonConvert.SerializeObject(sendQueue);
                    //JsonSerializer ser = new JsonSerializer();
                    //ser.Serialize(new JsonTextWriter(new StreamWriter(stream)), sendQueue);
                    new StreamWriter(stream).Write(json);
                    stream.Flush();
                    stream.Close();
                    var response = request.GetResponse();
                     */
                }
                Thread.Sleep(10000);
            }
        }
    }
}
