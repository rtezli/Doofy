using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pixills.Net.Http
{
    public abstract class HttpRequestHeader
    {
        public Version Version { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public string Resource { get; set; }
        public string ContentType { get; set; }
        public uint ContentLength { get; set; }

        public Dictionary<string, string> Fields = new Dictionary<string, string>()
        {
                {"Accept",null},
                {"Accept-Language",null},
                {"User-Agent",null},
                {"Accept-Encoding",null},
                {"Host",null},
                {"Connection",null},
                {"Accept-Charset",null},

                {"Cache-Control",null},
                {"Content-Type",null},
                {"Content-Length",null},
                {"Date",null},
                {"Expect",null},
                {"From",null},
                {"If-Modified-Since",null},
                {"If-None-Match",null},
                {"If-Range",null},
                {"If-Unmodified-Since",null},
                {"Max-Forwards",null},
                {"Pragma",null},
                {"Proxy-Authorization", null},
                {"Proxy-Connection",null},
                {"Range",null},
                {"Referer",null},
                {"TE", null},
                {"UA-CPU",null},
                {"Upgrade",null},

                {"Via",null},
                {"Warning",null},
                {"Cookie",""}
        };

        internal void TryParse(string p)
        {
            try
            {
                var HeaderFields = p.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                var MethodResourceversion = HeaderFields[0];
                var MethodResourceversionSplitted = MethodResourceversion.Split(' ');

                Method = MethodResourceversionSplitted[0];
                Resource = MethodResourceversionSplitted[1];
                Version = Version.Parse(MethodResourceversionSplitted[2].ToLower().Replace("http/",""));

                for (int i = 1; i < HeaderFields.Length; i++)
                {
                    var PropertyValue = HeaderFields[i].Split(new String[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

                    var key = PropertyValue[0].Trim().ToLower();
                    var value = PropertyValue[1].Trim();

                    if (Fields.Any(v => v.Key.ToLower().Equals(key)) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        Fields[key] = value;
                }

                if (!string.IsNullOrEmpty(Fields["Content-Type"]))
                    ContentType = Fields["Content-Type"];
                if (!string.IsNullOrEmpty(Fields["Content-Length"]))
                    ContentLength = Convert.ToUInt32(Fields["Content-Length"]);
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        internal void TryProxyParse(string p)
        {
            try
            {
                var HeaderFields = p.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                var MethodResourceversion = HeaderFields[0];
                var MethodResourceversionSplitted = MethodResourceversion.Split(' ');

                Method = MethodResourceversionSplitted[0];
                Resource = MethodResourceversionSplitted[1];
                Url = Resource;
                this.Version = Version.Parse(MethodResourceversionSplitted[2].ToLower().Replace("http/", ""));

                for (int i = 1; i < HeaderFields.Length; i++)
                {
                    var PropertyValue = HeaderFields[i].Split(new String[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

                    var key = PropertyValue[0].Trim();
                    var value = PropertyValue[1].Trim();

                    if (Fields.ContainsKey(key) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    {

                        if (key == "Proxy-Connection")
                        {
                            Fields["Connection"] = value;
                        }

                        else if (key == "UA-CPU")
                        {
                            Fields["UA-CPU"] = value;
                        }
                        else
                        {
                            Fields[key] = value;
                        }
                    }

                }
                if (Fields["Host"] == null)
                {
                    var breakupresource = this.Resource.Split('/');
                    Fields["Host"] = breakupresource[2];
                }
                if (Regex.IsMatch(this.Resource, "http://.*/"))
                {
                    var hostPart = "http://" + Fields["Host"];
                    Resource = this.Resource.Replace(hostPart, "");
                }

            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public byte[] Serialize()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1} {2}\r\n", this.Method, this.Resource, this.Version);

            foreach (var s in this.Fields)
            {
                if (s.Value != null)
                {
                    sb.AppendFormat("{0}: {1} \r\n", s.Key, s.Value);
                }
            }
            sb.Append("\r\n");
            return ASCIIEncoding.ASCII.GetBytes(sb.ToString());
        }
    }
}
