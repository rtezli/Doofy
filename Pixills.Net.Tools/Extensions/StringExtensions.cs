using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;

namespace Pixills.Net.Tools.Extensions
{
    public static class StringExtensions
    {
        public static byte[] AsHex(this string s)
        {
            return s.Select(c => { return (byte)c; }).ToArray();
        }

        public static string AsJson(this string s)
        {
            return JsonConvert.SerializeObject(s);
        }

        public static string AsUtf8Base64String(this string s)
        {
            return Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(s));
        }
    }
}
