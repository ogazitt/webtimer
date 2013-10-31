﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

using PacketDotNet;
using SharpPcap;

namespace WebTimer.Client
{
    public class CollectorClient
    {
        static List<Record> records = new List<Record>();
        static CaptureDeviceList devices = null;
        static object startingLock = new object();

        /// <summary>
        /// Start a network capture
        /// </summary>
        public static void Start()
        {
            lock (startingLock)
            {
                try
                {
                    // Retrieve the device list
                    devices = CaptureDeviceList.Instance;

                    // if capture is started on all devices, nothing to do
                    var started = true;
                    foreach (var dev in devices)
                    {
                        if (!dev.Started || !string.IsNullOrEmpty(dev.LastError))
                        {
                            started = false;
                            break;
                        }
                    }
                    if (started)
                        return;

                    // Print SharpPcap version
                    string ver = SharpPcap.Version.VersionString;
                    TraceLog.TraceInfo(string.Format("Starting collector with version {0}", ver));

                    // If no devices were found print an error
                    if (devices.Count < 1)
                    {
                        var error = "No devices were found on this machine";
                        TraceLog.TraceFatal(error);
                        throw new Exception(error);
                    }

                    foreach (var device in devices)
                    {
                        // reset device if already started
                        if (device.Started)
                        {
                            TraceLog.TraceInfo(string.Format("Stopping {0} {1}", device.Name, device.Description));
                            device.OnPacketArrival -= new PacketArrivalEventHandler(device_OnPacketArrival);
                            device.StopCapture();
                            device.Close();
                        }

                        // Register our handler function to the 'packet arrival' event
                        device.OnPacketArrival +=
                            new PacketArrivalEventHandler(device_OnPacketArrival);

                        // Open the device for capturing
                        int readTimeoutMilliseconds = 1000;
                        device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

                        TraceLog.TraceInfo(string.Format("Listening on {0} {1}",
                            device.Name, device.Description));

                        // DNS only
                        string filter = "udp dst port 53";
                        device.Filter = filter;

                        // Start the capturing process
                        device.StartCapture();
                    }
                }
                catch (Exception ex)
                {
                    TraceLog.TraceException("FATAL: Start caught exception", ex);
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Stop the network capture
        /// </summary>
        public static void Stop()
        {
            TraceLog.TraceInfo("Stopping collector");

            if (devices != null)
            {
                try
                {
                    foreach (var device in devices)
                    {
                        // Stop the capturing process
                        TraceLog.TraceInfo(string.Format("Stopping {0} {1}", device.Name, device.Description));
                        device.OnPacketArrival -= new PacketArrivalEventHandler(device_OnPacketArrival);
                        device.StopCapture();
                        device.Close();

                        // Print out the device statistics
                        TraceLog.TraceInfo(device.Statistics.ToString());
                    }
                }
                catch (Exception ex)
                {
                    TraceLog.TraceException("Stop caught exception", ex);
                }
            }

            TraceLog.TraceInfo("Stopped collector");
        }

        /// <summary>
        /// Get all the records since the last call
        /// This method removes the returned records from its internal buffer
        /// </summary>
        /// <returns>List of captured records</returns>
        public static List<Record> GetRecords()
        {
            try
            {
                lock (records)
                {
                    var recordList = new List<Record>(records);
                    records.Clear();
                    return recordList;
                }
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("GetRecords failed", ex);
                return null;
            }
        }

        #region Helpers

        private static void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            try
            {
                var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                if (packet is PacketDotNet.EthernetPacket)
                {
                    var eth = packet as EthernetPacket;

                    // this syntax is from PacketDotNet 0.13.0 which isn't supported in the nuget package (0.12.0)
                    //var udp = packet.Extract(typeof(UdpPacket)) as UdpPacket;
                    var udp = Extract(packet, typeof(UdpPacket)) as UdpPacket;
                    if (udp != null)
                    {
                        string websiteName = null;

                        if (udp.DestinationPort == 53) // DNS
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
                            websiteName = sb.ToString();
                        }

                        var hostMacAddress = eth.SourceHwAddress.ToString();
                        var hostIpAddress = "";
                        var hostName = "";
                        var destIpAddress = "";
                        var sourcePort = 0;
                        var destPort = 0;

                        var ip = packet.PayloadPacket as IPv4Packet;
                        if (ip != null)
                        {
                            hostIpAddress = ip.SourceAddress.ToString();
                            destIpAddress = ip.DestinationAddress.ToString();
                            sourcePort = udp.SourcePort;
                            destPort = udp.DestinationPort;
                            hostName = hostIpAddress;

                            try
                            {
                                IPHostEntry entry = null;
                                if (Environment.Version.CompareTo(new System.Version(4, 0)) < 0)
                                    entry = Dns.GetHostByAddress(ip.SourceAddress);
                                else
                                    entry = Dns.GetHostEntry(ip.SourceAddress);
                                if (entry != null)
                                    hostName = entry.HostName;
                            }
                            catch (Exception ex)
                            {
                                TraceLog.TraceException(String.Format("GetHostByAddress failed for {0}", hostIpAddress), ex);
                            }
                        }
                        var destMacAddress = eth.DestinationHwAddress.ToString();

                        // only log this level of detail if we're logging to the console
                        if (TraceLog.TraceDestination == TraceLog.Destination.Console)
                        {
                            TraceLog.TraceInfo(String.Format("Source: [{0}; {1}:{2}; {3}]; Dest: [{4}; {5}:{6}; Website: {7}",
                                hostMacAddress, hostIpAddress, sourcePort, hostName,
                                destMacAddress, destIpAddress, destPort,
                                websiteName));
                        }

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
            catch (Exception ex)
            {
                TraceLog.TraceException("OnPacketArrival caught exception", ex);
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

        #endregion
    }
}