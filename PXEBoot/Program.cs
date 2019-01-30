using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PXEBoot
{
    class Program
    {
        static UdpClient UDP4011 = null;
        static UdpClient UDP67 = null;
        public static UdpClient UDP69 = null;
        public static Sessions Session = null;
        static bool RunService = true;
#if !DEBUG
        static ServiceBase[] ServicesToRun;
#endif

        static IPAddress GetCurrentIP()
        {
            string strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            foreach (IPAddress a in addr)
            {
                if (a.AddressFamily == AddressFamily.InterNetwork)
                    return (a);
            }

            return (null);
        }

        static int Main(string[] args)
        {
            if (args.Length > 0)
            {
#if !DEBUG
                if (args[0] == "-install")
                {
                    ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                    return (0);
                }
                if (args[0] == "-console")
                {
                    SMain();
                    Console.WriteLine("Press any key . . . ");
                    Console.ReadKey(true);
                    StopService();
                    return (0);
                }
                if (args[0] == "-registereventlog")
                {
                    try
                    {
                        FoxEventLog.RegisterEventLog();
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine(ee.ToString());
                    }
                    return (0);
                }
                if (args[0] == "-createdirstruct")
                {
                    return (CreateDirStruct());
                }
#endif
            }

#if DEBUG
            SMain();
#else
            ServicesToRun = new ServiceBase[] 
            { 
                new PXEService() 
            };
            ServiceBase.Run(ServicesToRun);
#endif

            return (0);
        }

        public static void StopService()
        {
            RunService = false;

            if (UDP4011 != null)
                UDP4011.Close();
            if (UDP67 != null)
                UDP67.Close();
            if (UDP69 != null)
                UDP69.Close();
            if (Session != null)
                Session.StopSessions();
        }

        static void CreateDir(string Dir)
        {
            if (Directory.Exists(Settings.TFTPRootPath + Dir) == false)
                Directory.CreateDirectory(Settings.TFTPRootPath + Dir);
        }

        static int CreateDirStruct()
        {
            Console.WriteLine("Creating directory structure");
            try
            {
                Settings.Load();

                if (Directory.Exists(Settings.TFTPRootPath) == false)
                {
                    Console.WriteLine("Cannot find path " + (Settings.TFTPRootPath == null ? "<none>" : Settings.TFTPRootPath) + "\n\nMake sure that it is correct in the registry HKLM\\Software\\Fox\\PXEBoot\\RootPath");
                    return (1);
                }

                if (Settings.TFTPRootPath.EndsWith("\\") == false)
                    Settings.TFTPRootPath += "\\";

                Console.WriteLine("Directory: " + Settings.TFTPRootPath);

                CreateDir("ARC x86");
                CreateDir("DEC Alpha");
                CreateDir("EFI BC");
                CreateDir("EFI X64");
                CreateDir("EFI X86");
                CreateDir("EFI ITANIUM");
                CreateDir("EFI XScale");
                CreateDir("BIOS");
                CreateDir("NEC PC98");
                CreateDir("Unknown");

                Console.WriteLine("Done!");
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
                return (1);
            }

            return (0);
        }

        public static int SMain()
        {
            try
            {
                Settings.Load();

                if (Directory.Exists(Settings.TFTPRootPath) == false)
                {
                    FoxEventLog.WriteEventLog("Cannot find path " + (Settings.TFTPRootPath == null ? "<none>" : Settings.TFTPRootPath) + "\n\nMake sure that it is correct in the registry HKLM\\Software\\Fox\\PXEBoot\\RootPath", System.Diagnostics.EventLogEntryType.Error);
                    return (1);
                }

                if (Settings.TFTPRootPath.EndsWith("\\") == false)
                    Settings.TFTPRootPath += "\\";

                Session = new Sessions();

                UDP4011 = new UdpClient(new IPEndPoint(IPAddress.Any, 4011));
                UDP4011.EnableBroadcast = true;
                UDP4011.BeginReceive(new AsyncCallback(recv4011), UDP4011);
                UDP4011.DontFragment = true;

                UDP67 = new UdpClient(new IPEndPoint(IPAddress.Any, 67));
                UDP67.EnableBroadcast = true;
                UDP67.BeginReceive(new AsyncCallback(recv67), UDP67);
                UDP67.DontFragment = true;

                UDP69 = new UdpClient(new IPEndPoint(IPAddress.Any, 69));
                UDP69.EnableBroadcast = true;
                UDP69.BeginReceive(new AsyncCallback(recv69), UDP69);
                UDP69.DontFragment = true;

                Console.WriteLine("Ready...");
                FoxEventLog.WriteEventLog("Server started", System.Diagnostics.EventLogEntryType.Information);
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
                FoxEventLog.WriteEventLog("Server did not start: " + ee.Message, System.Diagnostics.EventLogEntryType.Error);
                return (1);
            }
#if DEBUG
            Console.WriteLine("Press any key . . . ");
            Console.ReadKey(true);
            StopService();
#endif
            return (0);
        }

        static void recv69(IAsyncResult res)
        {
            UdpClient u;
            try
            {
                u = (UdpClient)res.AsyncState;
            }
            catch
            {
                FoxEventLog.WriteEventLog("Invalid packet on UDP 69", System.Diagnostics.EventLogEntryType.Warning);
                return;
            }
            try
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
                byte[] buffer = u.EndReceive(res, ref ip);

                Session.Data(ip.Address, ip.Port, buffer);
            }
            catch
            {

            }
            finally
            {
                if (RunService == true)
                    u.BeginReceive(new AsyncCallback(recv69), u);
            }
        }

        static DHCPArchitecture DetectArch(string ID)
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

        static void recv4011(IAsyncResult res)
        {
            UdpClient u;
            try
            {
                u = (UdpClient)res.AsyncState;
            }
            catch
            {
                FoxEventLog.WriteEventLog("Invalid packet on UDP 4011", System.Diagnostics.EventLogEntryType.Warning);
                return;
            }
            try
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
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
                if (detectedarch == DHCPArchitecture.Undefined)
                    return;

                Session.RegisterSession(ip.Address, detectedarch);

                DHCPPacket send = new DHCPPacket();
                send.MacAddress = dhcppacket.MacAddress;
                send.XID = dhcppacket.XID;
                send.DHCP53MessageType = DHCPMessageType.DHCPACK;
                send.WantedDHCP9ParameterList = DHCPPacket.DHCP9ParameterListBootFiles;
                send.SupportedDHCP9ParameterList = dhcppacket.DHCP9ReqParameterList;
                send.DHCP60ClassIdentifier = "PXEClient";
                send.DHCP66BootServer = GetCurrentIP().ToString();

                string MACAddr = dhcppacket.GetMacAddress();
                string BootFile = "bootmgfw.efi";

                do
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Fox\\PXEBoot\\MAC\\" + MACAddr);
                    if (key != null)
                    {
                        object o = key.GetValue("BootFile");
                        if (o != null)
                        {
                            BootFile = Convert.ToString(o);
                            key.Close();
                            break;
                        }
                        key.Close();
                    }
                    if (MACAddr.Length <= 2)
                        break;
                    MACAddr = MACAddr.Substring(0, MACAddr.Length - 2);
                } while (MACAddr.Length > 0);

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
            }
            finally
            {
                if (RunService == true)
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
                            FoxEventLog.WriteEventLog("Cannot reset port 4011", System.Diagnostics.EventLogEntryType.Error);
                            Process.GetCurrentProcess().Kill();
                        }
                    }
                }
            }
        }

        static void recv67(IAsyncResult res)
        {
            UdpClient u;
            try
            {
                u = (UdpClient)res.AsyncState;
            }
            catch
            {
                FoxEventLog.WriteEventLog("Invalid packet on UDP 67", System.Diagnostics.EventLogEntryType.Warning);
                return;
            }

            DHCPPacket dhcppacket;
            DHCPArchitecture detectedarch;
            IPEndPoint ip;

            try
            {
                ip = new IPEndPoint(IPAddress.Any, 0);
                byte[] buffer = u.EndReceive(res, ref ip);

                dhcppacket = new DHCPPacket(buffer);
                detectedarch = DHCPArchitecture.Undefined;
            }
            catch
            {
                if (RunService == true) //Shutdown service causes an exception on u.EndReceive()
                    FoxEventLog.WriteEventLog("Cannot process data on UDP 67", System.Diagnostics.EventLogEntryType.Warning);
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

                DHCPPacket send = new DHCPPacket();
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
            }
            catch
            {
                FoxEventLog.WriteEventLog("Cannot fully process/decode data on UDP 67", System.Diagnostics.EventLogEntryType.Warning);
            }
            finally
            {
                if (RunService == true)
                    u.BeginReceive(new AsyncCallback(recv67), u);
            }
        }

        public static void SendData(UdpClient udp, int Port, IPAddress to, byte[] data)
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
    }
}
