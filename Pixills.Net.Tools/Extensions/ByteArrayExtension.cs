using System;
using System.Linq;
using System.Text;

namespace Pixills.Net.Tools.Extensions
{
    public static class ByteArrayExtension
    {
        public static string[] AsHexStringArray(this byte[] ba)
        {
            return ba.Select(b => { return string.Format("{0:d2}", b); }).ToArray();
        }

        public static string[] AsHexStringArray(this Array ba)
        {
            var array = ba as byte[];
            return array.Select(b => { return string.Format("{0:d2}", b); }).ToArray();
        }

        public static string AsHexString(this byte[] ba)
        {
            var s = ba.AsHexStringArray();
            var sb = new StringBuilder();
            if(ba.Length < 1){
                return string.Empty;
            }
            else if (ba.Length == 1)
            {
                return s[0];
            }
            else
            {
                for (int i = 0; i < s.Length - 1; i++)
                {
                    sb.AppendFormat("{0} ", s[i]);
                }
                sb.Append(ba[ba.Length - 1]);
                return sb.ToString();
            }
        }

        public static string AsHexString(this Array ba)
        {
            var s = ba.AsHexStringArray();
            var sb = new StringBuilder();
            if (ba.Length < 1)
            {
                return string.Empty;
            }
            else if (ba.Length == 1)
            {
                return s[0];
            }
            else
            {
                for (int i = 0; i < s.Length - 1; i++)
                {
                    sb.AppendFormat("{0} ", s[i]);
                }
                sb.Append(s[s.Length - 1]);
                return sb.ToString();
            }
        }
    }
}
