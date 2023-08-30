using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PXEBoot
{
    #region DHCPEnums

    enum DHCPHardwareType : byte
    {
        None = 0,
        Ethernet10 = 1,
        IEEE802Networks = 6,
        ARCNET = 7,
        LocalTalk = 11,
        LocalNet = 12,
        SMDS = 14,
        FrameRelay = 15,
        ATM = 16,
        HDLC = 17,
        FibreChannel = 18,
        ATM2 = 19,
        SerialLine = 20
    }

    enum DHCPOperationCode : byte
    {
        ClientToServer = 1,
        ServerToClient = 2
    }

    enum DHCPFlags : ushort
    {
        None = 0,
        Broadcast = 0x80
    }

    enum DHCPMessageType : byte
    {
        DHCPDISCOVER = 1,
        DHCPOFFER = 2,
        DHCPREQUEST = 3,
        DHCPACK = 5
    }

    enum DHCPArchitecture : ushort
    {
        Undefined = 0xFFFF,
        IA32Legacy = 0,
        NEC_PC98 = 1,
        EFI_ITANIUM = 2,
        DEC_ALPHA = 3,
        ARC_x86 = 4,
        Intel_Lean_Client = 5,
        EFI_IA32 = 6,
        EFI_ByteCode = 7,
        EFI_XScale = 8,
        EFI_EM64T = 9
    }

    #endregion

    class DHCPPacket
    {
        public bool Malformed = false;

        public static readonly List<byte> DHCP9ParameterListPXEClient = new List<byte>() { 60 };
        public static readonly List<byte> DHCP9ParameterListBootFiles = new List<byte>() { 60, 66, 67 };

        #region DHCP Structure

        struct DHCPOption
        {
            public byte Type;
            public byte Size;
            public byte[] Data;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DHCPHeader
        {
            public DHCPOperationCode OperationCode;
            public DHCPHardwareType HardwareType;
            public byte MACLength; //normally 6 bytes
            public byte Hops;
            public uint XID;
            public ushort Secs;
            public DHCPFlags Flags;
            public uint CIAddr; //Client IP Address
            public uint YIAddr; //"Your" IP Address
            public uint SIAddr; //Server IP Address
            public uint GIAddr; //Gateway IP Address
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] CHAddr; //Client Hardware Address
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] SName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] File; //Boot file
            public uint Magic; //Magic Cookie - LE: 63 53 82 63
        }

        DHCPHeader DHCPHdr = new DHCPHeader();
        List<DHCPOption> DHCPOptions = new List<DHCPOption>();

        #endregion

        #region Friendly Codes

        public string DHCP97ClientUUID;
        public string DHCP61ClientGUID;
        public List<byte> DHCP9ReqParameterList;
        public DHCPMessageType DHCP53MessageType;
        public int DHCP57MessageLength;
        public string DHCP60ClassIdentifier;
        public DHCPArchitecture DHCP93Architecture;
        public string DHCP94ClientNIC;
        public string DHCP66BootServer;
        public string DHCP67BootFilename;
        public byte[] DHCP43VendorSpecificInfo;
        
        public DHCPFlags Flags;
        public DHCPOperationCode OperationCode;
        public DHCPHardwareType HardwareType;
        public IPAddress IPClient;
        public IPAddress IPYours;
        public IPAddress IPServer;
        public IPAddress IPGateway;
        public byte[] MacAddress;
        public string Servername;
        public string BootFile;
        public uint XID;

        public List<byte> SupportedDHCP9ParameterList;
        public List<byte> WantedDHCP9ParameterList;

        #endregion

        DHCPHeader DecodeBytes(byte[] data)
        {
            try
            {
                GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                DHCPHeader temp = (DHCPHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DHCPHeader));
                handle.Free();
                return (temp);
            }
            catch (Exception ee)
            {
                Debug.WriteLine(ee.ToString());
            }
            return (new DHCPHeader());
        }

        byte[] EncodeBytes(DHCPHeader hdr)
        {
            try
            {
                int sz = Marshal.SizeOf(hdr);
                byte[] data = new byte[sz];
                IntPtr ptr = Marshal.AllocHGlobal(sz);
                Marshal.StructureToPtr(hdr, ptr, true);
                Marshal.Copy(ptr, data, 0, sz);
                Marshal.FreeHGlobal(ptr);
                return (data);
            }
            catch (Exception ee)
            {
                Debug.WriteLine(ee.ToString());
            }
            return (null);
        }

        public string GetMacAddress()
        {
            return (BytesToString(DHCPHdr.CHAddr, 0));
        }

        string BytesToString(byte[] data, int Start)
        {
            string tmp = "";
            for (int i = Start; i < data.Length - Start; i++)
                tmp += data[i].ToString("X2");
            return (tmp);
        }

        public DHCPPacket(byte[] data)
        {
            DHCPHdr = DecodeBytes(data);

            if (DHCPHdr.Magic != 0x63538263)
            {
                Malformed = true;
                return;
            }

            if (DHCPHdr.MACLength > 16)
            {
                Malformed = true;
                return;
            }

            for (int i = 0xF0; i < data.Length;)
            {
                if (data[i] == 0xFF)
                    break;

                if (data.Length - i < 2)
                {
                    Malformed = true;
                    break;
                }

                DHCPOption o = new DHCPOption();
                o.Type = data[i + 0];
                o.Size = data[i + 1];

                if (o.Type == 0 || o.Size == 0)
                {
                    Malformed = true;
                    break;
                }

                if (data.Length - (i + o.Size) < 0)
                {
                    Malformed = true;
                    break;
                }

                o.Data = new byte[o.Size];
                Buffer.BlockCopy(data, i + 2, o.Data, 0, o.Size);

                DHCPOptions.Add(o);

                i += 2 + o.Size;
            }

            foreach (DHCPOption o in DHCPOptions)
            {
                switch (o.Type)
                {
                    case 97:
                        if (o.Size != 17)
                        {
                            Malformed = true;
                            continue;
                        }
                        DHCP97ClientUUID = BytesToString(o.Data, 1);
                        break;
                    case 61:
                        //if (o.Size != 17)
                        //{
                        //    Malformed = true;
                        //    continue;
                        //}
                        DHCP61ClientGUID = BytesToString(o.Data, 1);
                        break;
                    case 55:
                        DHCP9ReqParameterList = new List<byte>();
                        foreach (byte b in o.Data)
                        {
                            if (b == 0 || b == 0xff)
                                Malformed = true;
                            if (DHCP9ReqParameterList.Contains(b) == false)
                                DHCP9ReqParameterList.Add(b);
                        }
                        break;
                    case 53:
                        if (o.Size != 1)
                        {
                            Malformed = true;
                            continue;
                        }
                        DHCP53MessageType = (DHCPMessageType)o.Data[0];
                        break;
                    case 57:
                        if (o.Size != 2)
                        {
                            Malformed = true;
                            continue;
                        }
                        DHCP57MessageLength = BitConverter.ToUInt16(o.Data.Reverse().ToArray(), 0);
                        break;
                    case 60:
                        if (o.Size != 32)
                        {
                            Malformed = true;
                            continue;
                        }
                        DHCP60ClassIdentifier = Encoding.ASCII.GetString(o.Data);
                        break;
                    case 66:
                        DHCP66BootServer = Encoding.ASCII.GetString(o.Data).NullTrim();
                        break;
                    case 67:
                        DHCP67BootFilename = Encoding.ASCII.GetString(o.Data).NullTrim();
                        break;
                    case 93:
                        if (o.Size != 2)
                        {
                            Malformed = true;
                            continue;
                        }
                        DHCP93Architecture = (DHCPArchitecture)BitConverter.ToUInt16(o.Data.Reverse().ToArray(), 0);
                        break;
                    case 94:
                        if (o.Size != 3)
                        {
                            Malformed = true;
                            continue;
                        }
                        if (o.Data[0] == 1)
                            DHCP94ClientNIC = "UNDI.";
                        else
                            DHCP94ClientNIC = "UNKN.";
                        DHCP94ClientNIC += o.Data[1].ToString("0") + "." + o.Data[2].ToString("0");
                        break;
                    default:
                        Debug.WriteLine("Unknown code: " + o.Type.ToString() + " (0x" + o.Type.ToString("X2") + ")");
                        break;
                }
            }

            Flags = DHCPHdr.Flags;
            OperationCode = DHCPHdr.OperationCode;
            HardwareType = DHCPHdr.HardwareType;
            XID = DHCPHdr.XID;
            IPClient = new IPAddress(DHCPHdr.CIAddr);
            IPYours = new IPAddress(DHCPHdr.YIAddr);
            IPServer = new IPAddress(DHCPHdr.SIAddr);
            IPGateway = new IPAddress(DHCPHdr.GIAddr);
            MacAddress = new byte[DHCPHdr.MACLength];
            Buffer.BlockCopy(DHCPHdr.CHAddr, 0, MacAddress, 0, DHCPHdr.MACLength);
            Servername = Encoding.ASCII.GetString(DHCPHdr.SName).NullTrim();
            BootFile = Encoding.ASCII.GetString(DHCPHdr.File).NullTrim();
        }

        public DHCPPacket(IPAddress IPServer)
        {
            this.IPServer = IPServer;
            OperationCode = DHCPOperationCode.ServerToClient;
            HardwareType = DHCPHardwareType.Ethernet10;
            Flags = 0;
        }

        public byte[] GetBytes()
        {
            if (this.MacAddress == null)
                return (null);
            if (this.MacAddress.Length > 16)
                return (null);
            if (this.SupportedDHCP9ParameterList == null || this.SupportedDHCP9ParameterList.Count == 0)
                return (null);
            if (this.WantedDHCP9ParameterList == null || this.WantedDHCP9ParameterList.Count == 0)
                return (null);
            if (this.Servername != null && this.Servername.Length > 64)
                return (null);
            if (this.BootFile != null && this.BootFile.Length > 128)
                return (null);

            DHCPHeader hdr = new DHCPHeader();
            hdr.OperationCode = this.OperationCode;
            hdr.HardwareType = this.HardwareType;
            hdr.Magic = 0x63538263;
            hdr.HardwareType = this.HardwareType;
            hdr.Flags = this.Flags;
            hdr.CIAddr = this.IPClient.GetAddressUint();
            hdr.GIAddr = this.IPGateway.GetAddressUint();
            hdr.YIAddr = this.IPYours.GetAddressUint();
            hdr.SIAddr = this.IPServer.GetAddressUint();
            hdr.XID = this.XID;
            hdr.MACLength = (byte)this.MacAddress.Length;
            hdr.CHAddr = new byte[16];
            Buffer.BlockCopy(this.MacAddress, 0, hdr.CHAddr, 0, this.MacAddress.Length);
            if (this.BootFile != null)
                hdr.File = Encoding.ASCII.GetBytes(this.BootFile.PadRight(128, '\0'));
            if (this.Servername != null)
                hdr.SName = Encoding.ASCII.GetBytes(this.Servername.PadRight(64, '\0'));

            byte[] headerdata = EncodeBytes(hdr);
            List<DHCPOption> DHCPOptions = new List<DHCPOption>();

            DHCPOption nonopto = new DHCPOption();
            nonopto.Type = 53;
            nonopto.Size = 1;
            nonopto.Data = new byte[1];
            nonopto.Data[0] = (byte)this.DHCP53MessageType;
            DHCPOptions.Add(nonopto);

            nonopto = new DHCPOption();
            nonopto.Type = 54;
            nonopto.Size = 4;
            nonopto.Data = this.IPServer.GetAddressBytes();
            DHCPOptions.Add(nonopto);


            foreach (byte code in SupportedDHCP9ParameterList)
            {
                if (WantedDHCP9ParameterList.Contains(code) == false)
                    continue;
                DHCPOption o = new DHCPOption();
                switch (code)
                {
                    case 60:
                        if (DHCP60ClassIdentifier == null)
                            continue;
                        o.Type = 60;
                        o.Data = Encoding.ASCII.GetBytes(DHCP60ClassIdentifier);
                        if (o.Data.Length > 128)
                            continue;
                        o.Size = (byte)o.Data.Length;
                        break;
                    case 66:
                        if (DHCP66BootServer == null)
                            continue;
                        o.Type = 66;
                        o.Data = Encoding.ASCII.GetBytes(DHCP66BootServer.Trim() + "\0");
                        if (o.Data.Length > 128)
                            continue;
                        o.Size = (byte)o.Data.Length;
                        break;
                    case 67:
                        if (DHCP67BootFilename == null)
                            continue;
                        o.Type = 67;
                        o.Data = Encoding.ASCII.GetBytes(DHCP67BootFilename.Trim() + "\0");
                        if (o.Data.Length > 128)
                            continue;
                        o.Size = (byte)o.Data.Length;
                        break;
                    case 43:
                        if (DHCP43VendorSpecificInfo == null)
                            continue;
                        o.Type = 43;
                        o.Data = DHCP43VendorSpecificInfo;
                        if (o.Data.Length > 128)
                            continue;
                        o.Size = (byte)o.Data.Length;
                        break;                    
                }
                DHCPOptions.Add(o);
            }

            int size = headerdata.Length;
            foreach (DHCPOption o in DHCPOptions)
            {
                size += 2 + o.Data.Length;
            }
            size++;

            byte[] data = new byte[size];
            Buffer.BlockCopy(headerdata, 0, data, 0, headerdata.Length);

            size = headerdata.Length;
            foreach (DHCPOption o in DHCPOptions)
            {
                data[size + 0] = o.Type;
                data[size + 1] = o.Size;
                Buffer.BlockCopy(o.Data, 0, data, size + 2, o.Data.Length);
                size += 2 + o.Data.Length;
            }

            data[data.Length - 1] = 0xFF;

            return (data);
        }
    }
}
