﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PXEBoot
{
#if DEBUG
    class Test
    {
        static byte[] DHCPdata = {
    0x01, 0x01, 0x06, 0x00, 0x2B, 0x9C, 0x4E, 0xCD, 0x00, 0x06, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x29, 0x9C, 
    0x4E, 0xCD, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x63, 0x82, 0x53, 0x63, 
    0x35, 0x01, 0x01, 0x37, 0x18, 0x01, 0x02, 0x03, 0x05, 0x06, 0x0B, 0x0C, 0x0D, 0x0F, 0x10, 0x11, 
    0x12, 0x2B, 0x36, 0x3C, 0x43, 0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x39, 0x02, 0x04, 
    0xEC, 0x61, 0x11, 0x00, 0x56, 0x4D, 0x50, 0x81, 0x74, 0x47, 0xF0, 0x6A, 0x98, 0xFD, 0xA3, 0xA3, 
    0xED, 0x9C, 0x4E, 0xCD, 0x5D, 0x02, 0x00, 0x00, 0x5E, 0x03, 0x01, 0x02, 0x01, 0x3C, 0x20, 0x50, 
    0x58, 0x45, 0x43, 0x6C, 0x69, 0x65, 0x6E, 0x74, 0x3A, 0x41, 0x72, 0x63, 0x68, 0x3A, 0x30, 0x30, 
    0x30, 0x30, 0x30, 0x3A, 0x55, 0x4E, 0x44, 0x49, 0x3A, 0x30, 0x30, 0x32, 0x30, 0x30, 0x31, 0xFF, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
    0x00, 0x00, 0x00, 0x00};

        static byte[] TFTPReqData = {
    0x00, 0x01, 0x62, 0x6F, 0x6F, 0x74, 0x6D, 0x67, 0x66, 0x77, 0x2E, 0x65, 0x66, 0x69, 0x00, 0x6F, 
    0x63, 0x74, 0x65, 0x74, 0x00, 0x74, 0x73, 0x69, 0x7A, 0x65, 0x00, 0x30, 0x00, 0x62, 0x6C, 0x6B, 
    0x73, 0x69, 0x7A, 0x65, 0x00, 0x31, 0x34, 0x36, 0x38, 0x00};

        static byte[] TFTPErrorData ={
    0x00,0x05,0x00,0x08,0x55,0x73,0x65,0x72,0x20,0x61,0x62,0x6f,0x72,0x74,0x65,0x64,0x20,0x74,0x68,
    0x65,0x20,0x74,0x72,0x61,0x6e,0x73,0x66,0x65,0x72,0x00};



        public static void Run()
        {
            DHCPPacket dh = new DHCPPacket(DHCPdata);
            TFTPPacketReadReq pp = new TFTPPacketReadReq(TFTPReqData);
            TFTPPacketError err = new TFTPPacketError(TFTPErrorData);
        }

        public static void DisplayIPv4NetworkInterfaces()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            Console.WriteLine("IPv4 interface information for {0}.{1}", properties.HostName, properties.DomainName);
            Console.WriteLine();

            foreach (NetworkInterface adapter in nics)
            {
                // Only display informatin for interfaces that support IPv4.
                if (adapter.Supports(NetworkInterfaceComponent.IPv4) == false)
                {
                    continue;
                }
                Console.WriteLine(adapter.Description);
                // Underline the description.
                Console.WriteLine(String.Empty.PadLeft(adapter.Description.Length, '='));
                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                // Try to get the IPv4 interface properties.
                IPv4InterfaceProperties p = adapterProperties.GetIPv4Properties();

                if (p == null)
                {
                    Console.WriteLine("No IPv4 information is available for this interface.");
                    Console.WriteLine();
                    continue;
                }
                // Display the IPv4 specific data.
                Console.WriteLine("  Index ............................. : {0}", p.Index);
                Console.WriteLine("  MTU ............................... : {0}", p.Mtu);
                Console.WriteLine("  APIPA active....................... : {0}",
                    p.IsAutomaticPrivateAddressingActive);
                Console.WriteLine("  APIPA enabled...................... : {0}",
                    p.IsAutomaticPrivateAddressingEnabled);
                Console.WriteLine("  Forwarding enabled................. : {0}",
                    p.IsForwardingEnabled);
                Console.WriteLine("  Uses WINS ......................... : {0}",
                    p.UsesWins);
                Console.WriteLine();
            }
        }
    }
#endif
}
