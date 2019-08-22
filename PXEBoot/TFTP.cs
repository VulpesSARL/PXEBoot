using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXEBoot
{
    public enum TFTPOpcode : short
    {
        Read = 1,
        Write = 2,
        DataPacket = 3,
        Ack = 4,
        Error = 5,
        OptionAck = 6
    }

    public enum TFTPErrorCode : short
    {
        NotDefined = 0,
        FileNotFound = 1,
        AccessViolation = 2,
        DiskFull = 3,
        IllegalTFTPOP = 4,
        UnknownTransferID = 5,
        FileAlreadyExist = 6,
        NoSuchUser = 7
    }

    abstract class TFTPPacket
    {
        protected TFTPOpcode Opcode;
        protected List<string> Data;
        public bool Malformed = false;
        public bool Unsupported = false;
        public bool WrongType = false;

        protected void DecodePacket(byte[] data, int offset)
        {
            if (data == null)
            {
                Malformed = true;
                return;
            }

            if (data.Length < offset)
            {
                Malformed = true;
                return;
            }

            Data = new List<string>();

            Opcode = (TFTPOpcode)(((int)data[0] * 0x100) + ((int)data[1] * 0x1));

            List<byte> buffer = new List<byte>();

            for (int i = offset; i < data.Length; i++)
            {
                if (data[i] == 0)
                {
                    if (buffer.Count > 0)
                    {
                        Data.Add(Encoding.ASCII.GetString(buffer.GetBytes()));
                        buffer = new List<byte>();
                    }
                    else
                    {
                        if (Data.Count == 0)
                            Data.Add("");
                    }
                }
                else
                {
                    buffer.Add(data[i]);
                }
            }

            if (buffer.Count > 0)
            {
                Malformed = true;
                Data.Add(Encoding.ASCII.GetString(buffer.GetBytes()));
                buffer = new List<byte>();
            }
        }
    }

    class TFTPPacketError : TFTPPacket
    {
        TFTPErrorCode ErrorCode;
        string Text;

        public TFTPPacketError(byte[] data)
        {
            DecodePacket(data, 4);
            if (Malformed == true)
                return;

            if (Opcode != TFTPOpcode.Error)
            {
                WrongType = true;
                Malformed = true;
                return;
            }

            ErrorCode = (TFTPErrorCode)(((int)data[2] * 0x100) + ((int)data[3] * 0x1));

            if (Data.Count > 0)
                Text = Data[0];
        }

        public TFTPPacketError(TFTPErrorCode Code, string text)
        {
            Text = text;
            ErrorCode = Code;
        }

        public byte[] GetBytes
        {
            get
            {
                byte[] str = Encoding.ASCII.GetBytes(Text);
                byte[] d = new byte[str.Length + 5];
                d[0] = 0;
                d[1] = 5;
                d[2] = (byte)(((ushort)ErrorCode & 0xFF00u) >> 8);
                d[3] = (byte)((ushort)ErrorCode & 0xFFu);
                Buffer.BlockCopy(str, 0, d, 4, str.Length);
                return (d);
            }
        }
    }

    class TFTPPacketClientAck : TFTPPacket
    {
        public ushort Ack;
        public TFTPPacketClientAck(byte[] data)
        {
            if (data.Length < 2)
            {
                Malformed = true;
                return;
            }

            Opcode = (TFTPOpcode)(((int)data[0] * 0x100) + ((int)data[1] * 0x1));

            if (Opcode != TFTPOpcode.Ack)
            {
                WrongType = true;
                Malformed = true;
                return;
            }

            if (data.Length < 4)
            {
                Malformed = true;
                return;
            }

            Ack = (ushort)(((int)data[2] * 0x100) + ((int)data[3] * 0x1));
        }
    }

    class TFTPPacketReadReq : TFTPPacket
    {
        public string Filename;
        public string Mode = "octet";
        public int? tsize = null;
        public int? blksize = null;
        public int? windowsize = null;
        public TFTPPacketReadReq(byte[] data)
        {
            DecodePacket(data, 2);
            if (Malformed == true)
                return;

            if (Opcode != TFTPOpcode.Read)
            {
                WrongType = true;
                Malformed = true;
                return;
            }
            if (Data.Count < 1)
            {
                Malformed = true;
                return;
            }

            Filename = Data[0];
            if (Data.Count > 1)
            {
                Mode = Data[1];
                if (Mode.ToLower().Trim() != "octet")
                    Unsupported = true;
            }

            if (Data.Count > 2)
            {
                for (int i = 2; i < Data.Count; i += 2)
                {
                    if (Data.Count >= i + 1)
                    {
                        if (Data[i + 0].ToLower() == "tsize")
                        {
                            int s;
                            if (int.TryParse(Data[i + 1], out s) == false)
                                Malformed = true;
                            tsize = s;
                            if (tsize < 0)
                                Malformed = true;
                        }
                        if (Data[i + 0].ToLower() == "blksize")
                        {
                            int s;
                            if (int.TryParse(Data[i + 1], out s) == false)
                                Malformed = true;
                            blksize = s;
                            if (blksize < 1 || blksize > 65535)
                                Malformed = true;
                        }
                        if (Data[i + 0].ToLower() == "windowsize")
                        {
                            int s;
                            if (int.TryParse(Data[i + 1], out s) == false)
                                Malformed = true;
                            windowsize = s;
                            if (windowsize < 1 || windowsize > 65535)
                                Malformed = true;
                        }
                    }
                }
            }
        }
    }

    class TFTPPacketOptAck
    {
        List<string> Options;

        public TFTPPacketOptAck(List<string> options)
        {
            Options = options;
        }

        public byte[] GetBytes
        {
            get
            {
                int sz = 2;
                foreach (string o in Options)
                {
                    sz += o.Length + 1;
                }

                byte[] d = new byte[sz];
                d[0] = 0;
                d[1] = 6;

                sz = 2;

                foreach (string o in Options)
                {
                    byte[] s = Encoding.ASCII.GetBytes(o + "\0");
                    Buffer.BlockCopy(s, 0, d, sz, s.Length);
                    sz += s.Length;
                }

                return (d);
            }
        }
    }

    class TFTPPacketWriteReq : TFTPPacket
    {
        public string Filename;
        public string Mode = "octet";
        public int? tsize = null;
        public int? blksize = null;
        public int? windowsize = null;
        public TFTPPacketWriteReq(byte[] data)
        {
            DecodePacket(data, 2);
            if (Malformed == true)
                return;

            if (Opcode != TFTPOpcode.Write)
            {
                WrongType = true;
                Malformed = true;
                return;
            }
            if (Data.Count < 1)
            {
                Malformed = true;
                return;
            }

            Filename = Data[0];
            if (Data.Count > 1)
            {
                Mode = Data[1];
                if (Mode.ToLower().Trim() != "octet")
                    Unsupported = true;
            }

            if (Data.Count > 2)
            {
                for (int i = 2; i < Data.Count; i += 2)
                {
                    if (Data.Count >= i + 1)
                    {
                        if (Data[i + 0].ToLower() == "tsize")
                        {
                            int s;
                            if (int.TryParse(Data[i + 1], out s) == false)
                                Malformed = true;
                            tsize = s;
                            if (tsize < 0)
                                Malformed = true;
                        }
                        if (Data[i + 0].ToLower() == "blksize")
                        {
                            int s;
                            if (int.TryParse(Data[i + 1], out s) == false)
                                Malformed = true;
                            blksize = s;
                            if (blksize < 1 || blksize > 65535)
                                Malformed = true;
                        }
                        if (Data[i + 0].ToLower() == "windowsize")
                        {
                            int s;
                            if (int.TryParse(Data[i + 1], out s) == false)
                                Malformed = true;
                            windowsize = s;
                            if (windowsize < 1 || windowsize > 65535)
                                Malformed = true;
                        }
                    }
                }
            }
        }
    }

    class TFTPData
    {
        ushort seq;
        byte[] data;

        public TFTPData(ushort seqid, byte[] Data)
        {
            seq = seqid;
            data = Data;
        }

        public byte[] GetBytes
        {
            get
            {
                byte[] d = new byte[(data != null ? data.Length : 0) + 4];
                d[0] = 0;
                d[1] = 3;
                if (data != null)
                {
                    d[2] = (byte)((seq & 0xFF00) >> 8);
                    d[3] = (byte)(seq & 0xFF);
                    Buffer.BlockCopy(data, 0, d, 4, data.Length);
                }
                else
                {
                    d[2] = d[3] = 0;
                }
                return (d);
            }
        }
    }
}
