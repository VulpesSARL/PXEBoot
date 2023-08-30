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
using System.Windows.Forms;

namespace PXEBoot
{
    class Program
    {
        static public bool RunService = true;
        static List<Connector> Connectors = new List<Connector>();
#if !DEBUG
        static ServiceBase[] ServicesToRun;
#endif

        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length > 0)
            {
#if !DEBUG
                if (args[0] == "/?" || args[0] == "/help" || args[0] == "-?" || args[0] == "-help")
                {
                    Console.WriteLine("/config            Starts GUI Configuration");
                    Console.WriteLine("/console           Run the application without service (debug)");
                    Console.WriteLine("/createdirstruct   Creates directory structure");
                    Console.WriteLine("/install           Installs the service");
                    Console.WriteLine("/registereventlog  Registers the Eventlog");
                    return (0);
                }
                if (args[0] == "/config")
                {
                    Console.WriteLine("Starting GUI Configuration");
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new frmConfigWindow());
                    return (0);
                }
                if (args[0] == "/install")
                {
                    ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                    return (0);
                }
                if (args[0] == "/console")
                {
                    SMain();
                    Console.WriteLine("Press any key . . . ");
                    Console.ReadKey(true);
                    StopService();
                    return (0);
                }
                if (args[0] == "/registereventlog")
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
                if (args[0] == "/createdirstruct")
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

        public static Dictionary<IPAddress, string> GetNICsAndIPAddresses()
        {
            Dictionary<IPAddress, string> dict = new Dictionary<IPAddress, string>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.Supports(NetworkInterfaceComponent.IPv4) == false)
                    continue;
                string NN = nic.Name;
                IPv4InterfaceProperties ipprop = nic.GetIPProperties().GetIPv4Properties();
                if (ipprop == null)
                    continue;

                foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;
                    if (ip.Address.ToString().StartsWith("169.254.") == true)
                        continue;
                    if (ip.Address.ToString() == "127.0.0.1")
                        continue;
                    dict.Add(ip.Address, NN + " " + ip.Address.ToString() + (ipprop.IsDhcpEnabled == true ? " [DHCP]" : ""));
                }
            }
            return (dict);
        }

        public static int SMain()
        {
            try
            {
                Settings.Load();

                if (Directory.Exists(Settings.TFTPRootPath) == false)
                {
                    FoxEventLog.WriteEventLog("Cannot find path " + (Settings.TFTPRootPath == null ? "<none>" : Settings.TFTPRootPath) + "\n\nMake sure that it is correct in the registry HKLM\\Software\\Fox\\PXEBoot\\RootPath", EventLogEntryType.Error);
                    return (1);
                }

                if (Settings.TFTPRootPath.EndsWith("\\") == false)
                    Settings.TFTPRootPath += "\\";

                foreach (KeyValuePair<IPAddress, string> kvp in GetNICsAndIPAddresses())
                {
                    if (Settings.UseAllInterfaces == true || Settings.Interfaces.Contains(kvp.Value) == true)
                    {
                        Connector conn = new Connector();
                        conn.Connect(kvp.Key);
                        Connectors.Add(conn);
                    }
                }

                if (Connectors.Count == 0)
                {
                    FoxEventLog.WriteEventLog("No interfaces available to listen to ...", EventLogEntryType.Error);
                    return (2);
                }

                Console.WriteLine("Ready...");
                FoxEventLog.WriteEventLog("Server started", EventLogEntryType.Information);
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
                FoxEventLog.WriteEventLog("Server did not start: " + ee.Message, EventLogEntryType.Error);
                return (3);
            }
#if DEBUG
            Console.WriteLine("Press any key . . . ");
            Console.ReadKey(true);
            StopService();
#endif
            return (0);
        }

        static public void StopService()
        {
            RunService = false;
            foreach (Connector conn in Connectors)
            {
                conn.StopService();
            }
        }
    }
}
