using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXEBoot
{
    using System;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;

    public sealed class VulpesUDPClient : IDisposable
    {
        private readonly Socket socket;

        public VulpesUDPClient(IPEndPoint ep, bool NoBCastNoReuse = false)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (NoBCastNoReuse == false)
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            }
            // Enable IP_PKTINFO
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
            socket.Bind(ep);
        }

        public ReceivedDatagram Receive()
        {
            byte[] buffer = new byte[2048];
            SocketFlags flags = SocketFlags.None;
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            IPPacketInformation pktInfo;

            Int32 received = socket.ReceiveMessageFrom(buffer, 0, buffer.Length, ref flags, ref remoteEP, out pktInfo);

            ReceivedDatagram dg = new ReceivedDatagram(buffer, received, (IPEndPoint)remoteEP, new LocalInterfaceData(pktInfo.Interface, pktInfo.Address));

            return (dg);
        }

        public void Send(byte[] data, IPEndPoint remote, LocalInterfaceData localInterface)
        {
            if (remote.Address == IPAddress.Broadcast)
            {
                for (int i = 0; i < localInterface.InterfaceIPAddresses.Length; i++)
                {
                    try
                    {
                        using (Socket reply = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                        {
                            reply.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                            reply.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                            reply.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
                            reply.Bind(new IPEndPoint(localInterface.InterfaceIPAddresses[i], ((IPEndPoint)socket.LocalEndPoint).Port));
                            reply.SendTo(data, remote);
                        }
                    }
                    catch
                    {
                        socket.SendTo(data, remote);
                    }
                }
            }
            else
            {
                socket.SendTo(data, remote);
            }
        }

        public void Send(byte[] data, int length, IPEndPoint remote, LocalInterfaceData localInterface)
        {
            byte[] b = new byte[length];
            Array.Copy(data, b, length > data.Length ? data.Length : length);
            Send(b, remote, localInterface);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, Object state)
        {
            AsyncReceiveState s = new AsyncReceiveState
            {
                Buffer = new Byte[2048],
                RemoteEP = new IPEndPoint(IPAddress.Any, 0),
                Flags = SocketFlags.None,
                UserState = state,
                Client = this
            };

            return (socket.BeginReceiveMessageFrom(s.Buffer, 0, s.Buffer.Length, s.Flags, ref s.RemoteEP, callback, s));
        }

        public ReceivedDatagram EndReceive(IAsyncResult ar)
        {
            AsyncReceiveState s = (AsyncReceiveState)ar.AsyncState;

            IPPacketInformation pktInfo;
            Int32 received = socket.EndReceiveMessageFrom(ar, ref s.Flags, ref s.RemoteEP, out pktInfo);

            return (new ReceivedDatagram(s.Buffer, received, (IPEndPoint)s.RemoteEP, new LocalInterfaceData(pktInfo.Interface, pktInfo.Address)));
        }

        public void Dispose()
        {
            try
            {
                socket?.Close();
            }
            catch
            {
            }
        }

        public void Close()
        {
            try
            {
                socket?.Close();
            }
            catch
            {
            }
        }

        public sealed class LocalInterfaceData
        {
            public int InterfaceID { get; }
            public IPAddress[] InterfaceIPAddresses { get; }
            public IPAddress[] BroadcastIPAddresses { get; }
            public string InterfaceName { get; }
            public IPAddress OriginatingIPAddress { get; }

            static Dictionary<int, LocalInterfaceData> NicCache = new Dictionary<int, LocalInterfaceData>();
            static object NicCacheLock = new object();

            public LocalInterfaceData(int InterfaceID, IPAddress OriginatingIPAddress)
            {
                this.OriginatingIPAddress = OriginatingIPAddress;
                this.InterfaceID = InterfaceID;

                LocalInterfaceData cached = null;

                lock (NicCacheLock)
                {
                    if (NicCache.ContainsKey(InterfaceID) == true)
                        cached = NicCache[InterfaceID];
                }

                if (cached != null)
                {
                    InterfaceName = cached.InterfaceName;
                    InterfaceIPAddresses = cached.InterfaceIPAddresses;
                    BroadcastIPAddresses = cached.BroadcastIPAddresses;
                    return;
                }

                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    IPInterfaceProperties props = ni.GetIPProperties();
                    IPv4InterfaceProperties ipv4 = props.GetIPv4Properties();
                    if (ipv4 == null)
                        continue;

                    if (ipv4.Index == InterfaceID)
                    {
                        List<IPAddress> IPs = new List<IPAddress>();
                        List<IPAddress> BIPs = new List<IPAddress>();
                        foreach (UnicastIPAddressInformation ip in props.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily != AddressFamily.InterNetwork)
                                continue;
                            if (ip.Address.ToString().StartsWith("169.254.") == true)
                                continue;
                            if (ip.Address.ToString() == "127.0.0.1")
                                continue;
                            IPs.Add(ip.Address);
                            BIPs.Add(GetBroadcastAddress(ip));
                        }
                        InterfaceName = ni.Name;
                        InterfaceIPAddresses = IPs.ToArray();
                        BroadcastIPAddresses = BIPs.ToArray();
                        break;
                    }
                }

                lock (NicCacheLock)
                    NicCache.Add(InterfaceID, this);
            }

            IPAddress GetBroadcastAddress(UnicastIPAddressInformation uni)
            {
                Byte[] ipBytes = uni.Address.GetAddressBytes();
                Byte[] maskBytes = uni.IPv4Mask.GetAddressBytes();

                if (ipBytes.Length != 4 || maskBytes.Length != 4)
                    throw new NotSupportedException("IPv6 not supported for broadcast.");

                Byte[] broadcast = new Byte[4];

                for (Int32 i = 0; i < 4; i++)
                {
                    Byte invertedMask = (Byte)~maskBytes[i];
                    broadcast[i] = (Byte)(ipBytes[i] | invertedMask);
                }

                return (new IPAddress(broadcast));
            }
        }

        public sealed class ReceivedDatagram
        {
            public Byte[] Buffer { get; }
            public Int32 Length { get; }
            public IPEndPoint RemoteEndpoint { get; }
            public LocalInterfaceData LocalInterface { get; }

            public ReceivedDatagram(Byte[] data, Int32 length, IPEndPoint remote, LocalInterfaceData localInterface)
            {
                Buffer = data;
                Length = length;
                RemoteEndpoint = remote;
                LocalInterface = localInterface;
            }

            public string GetString()
            {
                return (RemoteEndpoint.ToString() + " " + LocalInterface.ToString() + " " + BitConverter.ToString(Buffer, 0, Length));
            }
        }

        public sealed class AsyncReceiveState
        {
            public Byte[] Buffer;
            public EndPoint RemoteEP;
            public SocketFlags Flags;
            public Object UserState;
            public VulpesUDPClient Client;
        }
    }
}
