using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PXEBoot.Properties;
using static PXEBoot.VulpesUDPClient;

namespace PXEBoot
{
    class Sessions
    {
        Dictionary<IPAddress, SessionData> RunningSessions = new Dictionary<IPAddress, SessionData>();
        Thread RepeaterThread;
        Thread TimeoutCollector;
        bool StopThreads = false;
        Connector Connector;

        public Sessions(Connector Connector)
        {
            this.Connector = Connector;
            RepeaterThread = new Thread(new ThreadStart(Repeater));
            TimeoutCollector = new Thread(new ThreadStart(Collector));
            RepeaterThread.Start();
            TimeoutCollector.Start();
        }

        void Repeater()
        {
            while (StopThreads == false)
            {
                lock (RunningSessions)
                {
                    foreach (KeyValuePair<IPAddress, SessionData> kvp in RunningSessions)
                    {
                        if (kvp.Value.datatosend == null)
                            continue;
                        if (kvp.Value.LastSend.AddSeconds(30) < DateTime.Now)
                        {
                            kvp.Value.LastSend = DateTime.Now;
                            foreach (byte[] b in kvp.Value.datatosend)
                                Connector.SendData(Connector.UDP69, kvp.Value.LocalInterface, 69, kvp.Key, b);
                        }
                    }
                }

                for (int i = 0; i < 1000; i++)
                {
                    Thread.Sleep(10);
                    if (StopThreads == true)
                        return;
                }
            }
        }

        void Collector()
        {
            while (StopThreads == false)
            {
                lock (RunningSessions)
                {
                    Dictionary<IPAddress, SessionData> Delete = new Dictionary<IPAddress, SessionData>();
                    foreach (KeyValuePair<IPAddress, SessionData> kvp in RunningSessions)
                    {
                        if (kvp.Value.LastUpdated.AddMinutes(30) < DateTime.Now)
                            Delete.Add(kvp.Key, kvp.Value);
                    }
                    foreach (KeyValuePair<IPAddress, SessionData> kvp in Delete)
                    {
                        kvp.Value.CloseUDP();
                        if (kvp.Value.currentfile != null)
                            kvp.Value.currentfile.Close();
                        RunningSessions.Remove(kvp.Key);
                        Debug.WriteLine("SESSION: Removing Session (cleanup): " + kvp.Key);
                    }
                }

                for (int i = 0; i < 3000; i++)
                {
                    Thread.Sleep(10);
                    if (StopThreads == true)
                        return;
                }
            }
        }

        public void StopSessions()
        {
            StopThreads = true;
            RepeaterThread.Join();
            TimeoutCollector.Join();
            foreach (KeyValuePair<IPAddress, SessionData> kvp in RunningSessions)
            {
                kvp.Value.CloseUDP();
                if (kvp.Value.currentfile != null)
                    kvp.Value.currentfile.Close();
            }
        }

        public bool RegisterSession(LocalInterfaceData LocalInterface, IPAddress Client, DHCPArchitecture Architecture, string PathOverride)
        {
            if (Client == IPAddress.Any)
                return (false);

            bool ShouldInitiateSecureBootRecovery = false;

            lock (RunningSessions)
            {
                if (RunningSessions.ContainsKey(Client) == true)
                {
                    if (RunningSessions[Client].currentfile != null)
                        RunningSessions[Client].currentfile.Close();
                    if (RunningSessions[Client].AllowSecureBootRecovery == true && RunningSessions[Client].SecureBootRecoveryInProgress == false &&
                        string.IsNullOrWhiteSpace(RunningSessions[Client].LastFilenameRequested) == false && RunningSessions[Client].LastFilenameRequested.ToLower() == "bootmgfw.efi")
                        ShouldInitiateSecureBootRecovery = true;
                    RunningSessions.Remove(Client);

                    Debug.WriteLine("SESSION: Removing Session: " + Client);
                }
            }

            SessionData ses = new SessionData(Connector.ListenIP, this);
            ses.Architecture = Architecture;
            ses.IP = Client;
            ses.LocalInterface = LocalInterface;
            ses.TFTPRootPath = Settings.TFTPRootPath;
            if (Settings.DisableSecureBootRecovery == false)
            {
                if (Architecture == DHCPArchitecture.EFI_ByteCode || Architecture == DHCPArchitecture.EFI_EM64T)
                    ses.AllowSecureBootRecovery = true;
            }

            if (ShouldInitiateSecureBootRecovery == true && ses.AllowSecureBootRecovery == true)
                ses.SecureBootRecoveryInProgress = true;

            if (string.IsNullOrWhiteSpace(PathOverride) == false)
            {
                ses.TFTPRootPath += PathOverride;
                if (ses.TFTPRootPath.EndsWith("\\") == false)
                    ses.TFTPRootPath += "\\";
                if (Directory.Exists(ses.TFTPRootPath) == false)
                {
                    PathOverride = "";
                    ses.TFTPRootPath = Settings.TFTPRootPath;
                }
            }

            if (string.IsNullOrWhiteSpace(PathOverride) == true)
            {
                switch (Architecture)
                {
                    case DHCPArchitecture.ARC_x86:
                        ses.TFTPRootPath += "ARC x86\\"; break;
                    case DHCPArchitecture.DEC_ALPHA:
                        ses.TFTPRootPath += "DEC Alpha\\"; break;
                    case DHCPArchitecture.EFI_ByteCode:
                        ses.TFTPRootPath += "EFI BC\\"; break;
                    case DHCPArchitecture.EFI_EM64T:
                        ses.TFTPRootPath += "EFI X64\\"; break;
                    case DHCPArchitecture.EFI_IA32:
                        ses.TFTPRootPath += "EFI X86\\"; break;
                    case DHCPArchitecture.EFI_ITANIUM:
                        ses.TFTPRootPath += "EFI ITANIUM\\"; break;
                    case DHCPArchitecture.EFI_XScale:
                        ses.TFTPRootPath += "EFI XScale\\"; break;
                    case DHCPArchitecture.IA32Legacy:
                        ses.TFTPRootPath += "BIOS\\"; break;
                    case DHCPArchitecture.NEC_PC98:
                        ses.TFTPRootPath += "NEC PC98\\"; break;
                    default:
                        ses.TFTPRootPath += "Unknown\\"; break;
                }
            }

            lock (RunningSessions)
                RunningSessions.Add(Client, ses);

            Debug.WriteLine("SESSION: New Session: " + Client);

            return (true);
        }

        public bool Data(LocalInterfaceData LocalInterface, IPAddress client, int Port, byte[] data)
        {
            SessionData ses;
            lock (RunningSessions)
            {
                if (RunningSessions.ContainsKey(client) == false)
                {
                    SessionData nses = new SessionData(Connector.ListenIP, this);
                    nses.Architecture = DHCPArchitecture.Undefined;
                    nses.IP = client;
                    nses.LocalInterface = LocalInterface;
                    nses.TFTPRootPath = Settings.TFTPRootPath + "Unknown\\";
                    RunningSessions.Add(client, nses);
                }

                ses = RunningSessions[client];
            }

            if (data.Length < 2)
            {
                TFTPPacketError err = new TFTPPacketError(TFTPErrorCode.IllegalTFTPOP, "Illegal TFTP OP");
                ses.SendData(Port, LocalInterface, client, err.GetBytes);
                return (false);
            }

            TFTPOpcode opcode = (TFTPOpcode)(((int)data[0] * 0x100) + ((int)data[1] * 0x1));
            switch (opcode)
            {
                case TFTPOpcode.Read:
                    {
                        TFTPPacketReadReq rr = new TFTPPacketReadReq(data);
                        if (rr.Malformed == true)
                        {
                            TFTPPacketError err = new TFTPPacketError(TFTPErrorCode.IllegalTFTPOP, "Malformed packet");
                            ses.SendData(Port, LocalInterface, client, err.GetBytes);
                            return (false);
                        }
                        if (string.IsNullOrWhiteSpace(rr.Filename) == true)
                            rr.Filename = ses.currentfilename;
                        if (string.IsNullOrWhiteSpace(rr.Filename) == true)
                        {
                            TFTPPacketError err = new TFTPPacketError(TFTPErrorCode.IllegalTFTPOP, "Malformed packet");
                            ses.SendData(Port, LocalInterface, client, err.GetBytes);
                            return (false);
                        }

                        ses.LastUpdated = DateTime.Now;
                        if (ses.currentfile != null)
                            ses.currentfile.Close();
                        ses.currentfile = null;
                        ses.datatosend = null;
                        ses.OpenACK = false;
                        ses.LastFilenameRequested = rr.Filename;
                        ses.currentfilename = rr.Filename;

                        Int64 FileLength = 0;

                        if (rr.Filename.ToLower() == "bootmgfw.efi" && ses.SecureBootRecoveryInProgress == true)
                        {
                            //SecureBootRecovery.efi file!

                            Console.WriteLine(client.ToString() + "(" + ses.Architecture.ToString() + ") requesting file: " + rr.Filename + " *** SECUREBOOT RECOVERY FILE ***");
                            ses.currentfile = new MemoryStream(Resources.SecureBootRecoveryEFI);
                            ses.blksize = rr.blksize == null ? 512 : rr.blksize.Value;
                            if (ses.blksize > 1468)
                                ses.blksize = 1468;
                            ses.Sequence = null;
                            ses.CurrentPos = 0;
                            ses.TotalSize = Resources.SecureBootRecoveryEFI.Length;
                            FileLength = ses.TotalSize;
                            ses.windowsize = rr.windowsize == null ? 1 : rr.windowsize.Value;
                        }
                        else
                        {
                            Console.WriteLine(client.ToString() + "(" + ses.Architecture.ToString() + ") requesting file: " + rr.Filename);
                            rr.Filename = rr.Filename.Replace("/", "\\");
                            if (rr.Filename.StartsWith("\\") == true)
                                rr.Filename = rr.Filename.Substring(1, rr.Filename.Length - 1);
                            if (rr.Filename.EndsWith("\\") == true)
                            {
                                TFTPPacketError err = new TFTPPacketError(TFTPErrorCode.FileNotFound, "Invalid path");
                                ses.SendData(Port, LocalInterface, client, err.GetBytes);
                                return (false);
                            }
                            if (File.Exists(ses.TFTPRootPath + rr.Filename) == false)
                            {
                                TFTPPacketError err = new TFTPPacketError(TFTPErrorCode.FileNotFound, "File does not exist");
                                ses.SendData(Port, LocalInterface, client, err.GetBytes);
                                return (false);
                            }
                            ses.currentfile = File.Open(ses.TFTPRootPath + rr.Filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                            ses.blksize = rr.blksize == null ? 512 : rr.blksize.Value;
                            if (ses.blksize > 1468)
                                ses.blksize = 1468;
                            ses.Sequence = null;
                            ses.CurrentPos = 0;
                            FileInfo info = new FileInfo(ses.TFTPRootPath + rr.Filename);
                            ses.TotalSize = info.Length;
                            FileLength = ses.TotalSize;
                            ses.windowsize = rr.windowsize == null ? 1 : rr.windowsize.Value;
                        }
                        List<string> Options = new List<string>();
                        if (rr.blksize != null)
                        {
                            Options.Add("blksize");
                            Options.Add(ses.blksize.ToString());
                        }
                        if (rr.tsize != null)
                        {
                            Options.Add("tsize");
                            Options.Add(FileLength.ToString());
                        }
                        if (rr.windowsize != null)
                        {
                            Options.Add("windowsize");
                            Options.Add(ses.windowsize.ToString());
                        }
                        TFTPPacketOptAck oack = new TFTPPacketOptAck(Options);
                        ses.OpenACK = true;
                        ses.SendData(Port, LocalInterface, client, oack.GetBytes);
                        Console.WriteLine(client.ToString() + "(" + ses.Architecture.ToString() + ") got file: " + ses.TFTPRootPath + rr.Filename);
                        break;
                    }
                case TFTPOpcode.Write:
                    {
                        TFTPPacketError err = new TFTPPacketError(TFTPErrorCode.AccessViolation, "Writing not supported");
                        ses.SendData(Port, LocalInterface, client, err.GetBytes);
                        break;
                    }
                case TFTPOpcode.Error:
                    {
                        //ses.LastUpdated = DateTime.Now;
                        //if (ses.currentfile != null)
                        //    ses.currentfile.Close();
                        //ses.currentfile = null;
                        //ses.datatosend = null;
                        //ses.OpenACK = false;
                        Console.WriteLine(client.ToString() + "(" + ses.Architecture.ToString() + ") got error from client");
                        break;
                    }
                case TFTPOpcode.Ack:
                    {
                        TFTPPacketClientAck ack = new TFTPPacketClientAck(data);
                        if (ack.Malformed == true)
                        {
                            TFTPPacketError err = new TFTPPacketError(TFTPErrorCode.IllegalTFTPOP, "Malformed packet");
                            ses.SendData(69, LocalInterface, client, err.GetBytes);
                            return (false);
                        }
                        ses.LastUpdated = DateTime.Now;

                        if (ses.Sequence == null)
                            ses.Sequence = ack.Ack;

                        if (ack.Ack < ses.Sequence)
                            return (true);

                        if (ack.Ack != ses.Sequence)
                        {
                            TFTPPacketError err = new TFTPPacketError(TFTPErrorCode.IllegalTFTPOP, "Invalid sequence");
                            ses.SendData(Port, LocalInterface, client, err.GetBytes);
                            ses.LastSend = DateTime.Now;
                            return (false);
                        }

                        ses.datatosend = new List<byte[]>();
                        long Read = ses.blksize;
                        for (int i = 0; i < ses.windowsize; i++)
                        {
                            ses.LastSend = DateTime.Now;

                            if (ses.CurrentPos + Read > ses.TotalSize)
                                Read = ses.TotalSize - ses.CurrentPos;
                            if (Read == 0)
                            {
                                break;
                            }
                            else
                            {
                                byte[] filedata = new byte[Read];
                                ses.currentfile.Read(filedata, 0, (int)Read);
                                ses.Sequence++;
                                TFTPData d = new TFTPData(ses.Sequence.Value, filedata);
                                ses.OpenACK = true;
                                ses.LastSend = DateTime.Now;
                                ses.datatosendport = Port;
                                ses.CurrentPos += Read;
                                ses.datatosend.Add(d.GetBytes);
                            }
                        }
                        foreach (byte[] b in ses.datatosend)
                        {
                            ses.SendData(Port, LocalInterface, client, b);
                        }

                        if (Read == 0)
                        {
                            ses.OpenACK = false;
                            ses.datatosend = null;
                            ses.LastSend = DateTime.Now;
                        }
                        break;
                    }
                default:
                    {
                        TFTPPacketError err = new TFTPPacketError(TFTPErrorCode.NotDefined, "Not defined OP " + opcode.ToString());
                        ses.SendData(Port, LocalInterface, client, err.GetBytes);
                        ses.OpenACK = false;
                        return (false);
                    }
            }

            return (true);
        }
    }

    class SessionData
    {
        public IPAddress IP;
        public LocalInterfaceData LocalInterface;
        public Stream currentfile = null;
        public string currentfilename = "";
        public int blksize = 512;
        public DateTime LastUpdated = DateTime.Now;
        public DateTime LastSend = DateTime.Now;
        public string LastFilenameRequested;
        public DHCPArchitecture Architecture;
        public string TFTPRootPath;
        public long TotalSize;
        public long CurrentPos;
        public ushort? Sequence = null;
        public List<byte[]> datatosend = null;
        public int datatosendport;
        public int windowsize;
        public bool AllowSecureBootRecovery = false;
        public bool SecureBootRecoveryInProgress = false;
        VulpesUDPClient udp;
        public bool OpenACK = false;
        IPAddress ListenIP;
        Sessions Session;

        public SessionData(IPAddress ListenIP, Sessions Session)
        {
            this.ListenIP = ListenIP;
            this.Session = Session;

            for (int i = 60000; i < 65531; i++)
            {
                try
                {
                    udp = new VulpesUDPClient(new IPEndPoint(ListenIP, i), true);
                    udp.BeginReceive(new AsyncCallback(recvdata), udp);
                    Debug.WriteLine("TFTP Listener: " + ListenIP + ":" + i);
                    break;
                }
                catch
                {
                    udp = null;
                }
            }
            if (udp == null)
                FoxEventLog.WriteEventLog("Cannot map a port for TFTP Data", EventLogEntryType.Error);
        }

        void recvdata(IAsyncResult res)
        {
            VulpesUDPClient u;

            try
            {
                u = ((AsyncReceiveState)res.AsyncState).Client;
            }
            catch
            {
                FoxEventLog.WriteEventLog("Bizarre TFTP state", System.Diagnostics.EventLogEntryType.Warning);
                return;
            }

            try
            {
                if (OpenACK == false)
                    return;
                ReceivedDatagram recv = u.EndReceive(res);

                byte[] buffer = buffer = new byte[recv.Length];
                Array.Copy(recv.Buffer, buffer, recv.Length);

                Session.Data(recv.LocalInterface, recv.RemoteEndpoint.Address, recv.RemoteEndpoint.Port, buffer);
            }
            catch
            {

            }
            finally
            {
                try
                {
                    u.BeginReceive(new AsyncCallback(recvdata), u);
                }
                catch
                {

                }
            }
        }

        public void CloseUDP()
        {
            udp.Close();
        }

        public bool SendData(int Port, LocalInterfaceData LocalInterface, IPAddress to, byte[] data)
        {
            try
            {
                udp.Send(data, data.Length, new IPEndPoint(to, Port), LocalInterface);
                return (true);
            }
            catch (Exception ee)
            {
                SocketException eee = (SocketException)ee;
                Debug.WriteLine(eee.SocketErrorCode.ToString() + "\n" + ee.ToString());
                Console.WriteLine("Error sending data to " + to.ToString() + ": SZ=" + data.Length);
                return (false);
            }
        }
    }
}
