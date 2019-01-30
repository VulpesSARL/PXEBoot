using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PXEBoot
{
    static class Extensions
    {
        public static string NullTrim(this string str)
        {
            string s = str.Trim();
            while (s.StartsWith("\0") == true)
                s = s.Substring(1, s.Length - 1);
            s = s.Trim();
            while (s.EndsWith("\0") == true)
                s = s.Substring(0, s.Length - 1);
            s = s.Trim();
            return (s);
        }

        public static uint GetAddressUint(this IPAddress addr)
        {
            if (addr == null)
                return (0);
            byte[] d = addr.GetAddressBytes();
            return ((d[0] * 0x1u) + (d[1] * 0x100u) + (d[2] * 0x10000u) + (d[3] * 0x1000000u));
        }

        public static byte[] GetBytes(this List<byte> b)
        {
            if (b == null)
                return (new byte[0]);
            if (b.Count == 0)
                return (new byte[0]);
            byte[] bb = new byte[b.Count];
            for (int i = 0; i < b.Count; i++)
                bb[i] = b[i];
            return (bb);
        }
    }
}
