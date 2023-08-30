using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace PXEBoot
{
    internal class Connector
    {
        UdpClient UDP4011 = null;
        UdpClient UDP67 = null;
        public UdpClient UDP69 = null;
        Sessions Session = null;
        public IPAddress ListenIP = IPAddress.Any;

        public void Connect(IPAddress ListenIP)
        {
            this.ListenIP = ListenIP;
            Console.WriteLine("Binding: " + ListenIP.ToString());
            Debug.WriteLine("Binding: " + ListenIP.ToString());

            Session = new Sessions(this);

            UDP4011 = new UdpClient(new IPEndPoint(ListenIP, 4011));
            UDP4011.EnableBroadcast = true;
            UDP4011.DontFragment = true;
            UDP4011.BeginReceive(new AsyncCallback(recv4011), UDP4011);

            UDP67 = new UdpClient(new IPEndPoint(ListenIP, 67));
            UDP67.EnableBroadcast = true;
            UDP67.DontFragment = true;
            UDP67.BeginReceive(new AsyncCallback(recv67), UDP67);

            UDP69 = new UdpClient(new IPEndPoint(ListenIP, 69));
            UDP69.EnableBroadcast = true;
            UDP69.DontFragment = true;
            UDP69.BeginReceive(new AsyncCallback(recv69), UDP69);
        }

        public void StopService()
        {
            Console.WriteLine("Disconnecting: " + ListenIP.ToString());
            Debug.WriteLine("Binding: " + ListenIP.ToString());

            if (UDP4011 != null)
                UDP4011.Close();
            if (UDP67 != null)
                UDP67.Close();
            if (UDP69 != null)
                UDP69.Close();
            if (Session != null)
                Session.StopSessions();
        }

        void recv69(IAsyncResult res)
        {
            UdpClient u;
            try
            {
                u = (UdpClient)res.AsyncState;
            }
            catch
            {
                FoxEventLog.WriteEventLog("Invalid packet on UDP 69 on " + ListenIP.ToString(), EventLogEntryType.Warning);
                return;
            }
            try
            {
                IPEndPoint ip = new IPEndPoint(ListenIP, 0);
                byte[] buffer = u.EndReceive(res, ref ip);

                Session.Data(ip.Address, ip.Port, buffer);
            }
            catch
            {

            }
            finally
            {
                try
                {
                    if (Program.RunService == true)
                        u.BeginReceive(new AsyncCallback(recv69), u);
                }
                catch
                {
                    //reset port!
                    try
                    {
                        u.Close();
                    }
                    catch
                    {

                    }
                    UDP69 = new UdpClient(new IPEndPoint(ListenIP, 69));
                    UDP69.EnableBroadcast = true;
                    UDP69.DontFragment = true;
                    UDP69.BeginReceive(new AsyncCallback(recv69), UDP69);
                }
            }
        }

        void recv4011(IAsyncResult res)
        {
            UdpClient u;
            try
            {
                u = (UdpClient)res.AsyncState;
            }
            catch
            {
                FoxEventLog.WriteEventLog("Invalid packet on UDP 4011 on " + ListenIP.ToString(), EventLogEntryType.Warning);
                return;
            }
            try
            {
                IPEndPoint ip = new IPEndPoint(ListenIP, 0);
                byte[] buffer;

                try
                {
                    buffer = u.EndReceive(res, ref ip);
                }
                catch
                {
                    return;
                }

                DHCPPacket dhcppacket = new DHCPPacket(buffer);
                DHCPArchitecture detectedarch = DHCPArchitecture.Undefined;

                if (dhcppacket.Malformed == true)
                    return;
                if (dhcppacket.OperationCode != DHCPOperationCode.ClientToServer)
                    return;
                if (dhcppacket.DHCP53MessageType != DHCPMessageType.DHCPREQUEST)
                    return;
                if (dhcppacket.DHCP60ClassIdentifier.StartsWith("PXEClient") == false)
                    return;
                detectedarch = DetectArch(dhcppacket.DHCP60ClassIdentifier);
                //if (detectedarch == DHCPArchitecture.Undefined)
                //    return;

                string MACAddr = dhcppacket.GetMacAddress();
                string BootFile = "bootmgfw.efi";
                string BootPath = null;

                do
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Fox\\PXEBoot\\MAC\\" + MACAddr))
                    {
                        if (key != null)
                        {
                            object o = key.GetValue("BootFile");
                            if (o != null)
                                BootFile = Convert.ToString(o);

                            o = key.GetValue("Path");
                            if (o != null)
                            {
                                BootPath = Convert.ToString(o);
                            }
                            break;
                        }
                    }
                    if (MACAddr.Length <= 2)
                        break;
                    MACAddr = MACAddr.Substring(0, MACAddr.Length - 2);
                } while (MACAddr.Length > 0);

                Session.RegisterSession(ip.Address, detectedarch, BootPath);

                DHCPPacket send = new DHCPPacket(ListenIP);
                send.MacAddress = dhcppacket.MacAddress;
                send.XID = dhcppacket.XID;
                send.DHCP53MessageType = DHCPMessageType.DHCPACK;
                send.WantedDHCP9ParameterList = DHCPPacket.DHCP9ParameterListBootFiles;
                send.SupportedDHCP9ParameterList = dhcppacket.DHCP9ReqParameterList;
                send.DHCP60ClassIdentifier = "PXEClient";
                send.DHCP66BootServer = ListenIP.ToString();
                send.DHCP67BootFilename = BootFile;

                //send.BootFile = "bootmgfw.efi";
                //send.Servername = GetCurrentIP().ToString();

                if (detectedarch == DHCPArchitecture.IA32Legacy)
                {
                    send.DHCP43VendorSpecificInfo = new byte[] { 0x01, 0x04, 0x00, 0x00, 0x00, 0x00, 0xff };
                    send.WantedDHCP9ParameterList.Add(43);
                }

                byte[] data = send.GetBytes();
                if (data == null)
                    return;

                SendData(UDP4011, 4011, ip.Address, data);
                SendData(UDP4011, 68, IPAddress.Broadcast, data);

                Console.WriteLine(ip.Address.ToString() + " DHCP ack sent (System: " + detectedarch.ToString() + ")");
                Debug.WriteLine(ip.Address.ToString() + " DHCP ack sent (System: " + detectedarch.ToString() + ")");
            }
            finally
            {
                if (Program.RunService == true)
                {
                    try
                    {
                        u.BeginReceive(new AsyncCallback(recv4011), u);
                    }
                    catch
                    {
                        try
                        {
                            Thread.Sleep(1000);
                            u.BeginReceive(new AsyncCallback(recv4011), u);
                        }
                        catch
                        {
                            FoxEventLog.WriteEventLog("Cannot reset port 4011 on " + ListenIP.ToString(), EventLogEntryType.Error);
                            Process.GetCurrentProcess().Kill();
                        }
                    }
                }
            }
        }

        void recv67(IAsyncResult res)
        {
            UdpClient u;
            try
            {
                u = (UdpClient)res.AsyncState;
            }
            catch
            {
                FoxEventLog.WriteEventLog("Invalid packet on UDP 67 on " + ListenIP.ToString(), EventLogEntryType.Warning);
                return;
            }

            DHCPPacket dhcppacket;
            DHCPArchitecture detectedarch;
            IPEndPoint ip;

            try
            {
                ip = new IPEndPoint(ListenIP, 0);
                byte[] buffer = u.EndReceive(res, ref ip);

                dhcppacket = new DHCPPacket(buffer);
                detectedarch = DHCPArchitecture.Undefined;
            }
            catch
            {
                if (Program.RunService == true) //Shutdown service causes an exception on u.EndReceive()
                    FoxEventLog.WriteEventLog("Cannot process data on UDP 67 on " + ListenIP.ToString(), EventLogEntryType.Warning);
                return;
            }

            try
            {
                if (dhcppacket.Malformed == true)
                    return;
                if (dhcppacket.OperationCode != DHCPOperationCode.ClientToServer)
                    return;
                if (dhcppacket.DHCP53MessageType != DHCPMessageType.DHCPDISCOVER)
                    return;
                if (dhcppacket.DHCP60ClassIdentifier.StartsWith("PXEClient") == false)
                    return;
                detectedarch = DetectArch(dhcppacket.DHCP60ClassIdentifier);
                if (detectedarch == DHCPArchitecture.Undefined)
                    return;

                DHCPPacket send = new DHCPPacket(ListenIP);
                send.MacAddress = dhcppacket.MacAddress;
                send.XID = dhcppacket.XID;
                send.DHCP53MessageType = DHCPMessageType.DHCPOFFER;
                send.WantedDHCP9ParameterList = DHCPPacket.DHCP9ParameterListPXEClient;
                send.SupportedDHCP9ParameterList = dhcppacket.DHCP9ReqParameterList;
                send.DHCP60ClassIdentifier = "PXEClient";

                byte[] data = send.GetBytes();
                if (data == null)
                    return;
                SendData(UDP67, 68, IPAddress.Broadcast, data);

                Console.WriteLine(ip.Address.ToString() + " DHCP offer sent");
                Debug.WriteLine(ip.Address.ToString() + " DHCP offer sent");
            }
            catch
            {
                FoxEventLog.WriteEventLog("Cannot fully process/decode data on UDP 67 on " + ListenIP.ToString(), EventLogEntryType.Warning);
            }
            finally
            {
                if (Program.RunService == true)
                    u.BeginReceive(new AsyncCallback(recv67), u);
            }
        }

        public void SendData(UdpClient udp, int Port, IPAddress to, byte[] data)
        {
            try
            {
                udp.Send(data, data.Length, new IPEndPoint(to, Port));
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
        }

        DHCPArchitecture DetectArch(string ID)
        {
            DHCPArchitecture detectedarch = DHCPArchitecture.Undefined;
            if (ID.StartsWith("PXEClient:Arch:00000") == true)
                detectedarch = DHCPArchitecture.IA32Legacy;
            if (ID.StartsWith("PXEClient:Arch:00001") == true)
                detectedarch = DHCPArchitecture.NEC_PC98;
            if (ID.StartsWith("PXEClient:Arch:00002") == true)
                detectedarch = DHCPArchitecture.EFI_ITANIUM;
            if (ID.StartsWith("PXEClient:Arch:00003") == true)
                detectedarch = DHCPArchitecture.DEC_ALPHA;
            if (ID.StartsWith("PXEClient:Arch:00004") == true)
                detectedarch = DHCPArchitecture.ARC_x86;
            if (ID.StartsWith("PXEClient:Arch:00006") == true)
                detectedarch = DHCPArchitecture.EFI_IA32;
            if (ID.StartsWith("PXEClient:Arch:00007") == true)
                detectedarch = DHCPArchitecture.EFI_ByteCode;
            if (ID.StartsWith("PXEClient:Arch:00008") == true)
                detectedarch = DHCPArchitecture.EFI_XScale;
            if (ID.StartsWith("PXEClient:Arch:00009") == true)
                detectedarch = DHCPArchitecture.EFI_EM64T;
            return (detectedarch);
        }
    }
}
