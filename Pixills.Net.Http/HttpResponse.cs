using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Pixills.Tools.Log;
using System.IO;

namespace Pixills.Net.Http
{
    public class HttpResponse : HttpMessage
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public MemoryStream ResponseStream { get; set; }

        public byte[] RawPayload { get; set; }

        public HttpResponse()
            : base()
        {

        }

        public bool TryParseResponse(MemoryStream ms)
        {
            ResponseStream = ms;
            RawContent = ms.GetBuffer();
            var p = UTF8Encoding.UTF8.GetString(RawContent);

            string[] HeaderAndData = p.Split(new String[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                if (HeaderAndData.Length < 1)
                    throw new FormatException();

                var headerFields = HeaderAndData[0].Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                var MethodResourceversion = headerFields[0];
                var MethodResourceversionSplitted = MethodResourceversion.Split(' ');

                Version = Version.Parse(MethodResourceversionSplitted[0].ToLower().Replace("http/", ""));
                StatusCode = Convert.ToInt32(MethodResourceversionSplitted[1]);

                for (int i = 2; i < MethodResourceversionSplitted.Length; i++)
                    StatusMessage += MethodResourceversionSplitted[i] + " ";

                StatusMessage = StatusMessage.Trim();

                for (int i = 1; i < headerFields.Length; i++)
                {
                    var PropertyValue = headerFields[i].Split(new String[] { ": " }, StringSplitOptions.None);

                    var key = PropertyValue[0].Trim();
                    var value = PropertyValue[1].Trim();

                    if (Headers.Any(v => v.Key.ToLower().Equals(key.ToLower())) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        Headers[key] = value;

                    if (!Headers.ContainsKey(key))
                        HttpHandler.Log(LogLevel.Warning, "Key \"" + key + "\" is missing in header list");
                }

                if (!string.IsNullOrEmpty(Headers["Content-Type"]))
                    ContentType = Headers["Content-Type"];
                if (!string.IsNullOrEmpty(Headers["Content-Length"]))
                    ContentLength = Convert.ToInt32(Headers["Content-Length"]);

                if (HeaderAndData.Length >= 2)
                {
                    HasPayload = true;
                    var contentLength = RawContent.Length - HeaderAndData[0].Length - 4;
                    Content = new byte[contentLength];
                    Array.Copy(RawContent, RawContent.Length - contentLength, Content, 0, contentLength);
                }
                return true;
            }
            catch (Exception e)
            {
                HttpHandler.Log(LogLevel.Error, "Http request parsing error : " + e.Message);
                return false;
            }

        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("HTTP/{0} {1} {2}\r\n", Version, StatusCode, StatusMessage);
            foreach (var v in Headers)
            {
                if (v.Value != null)
                    sb.AppendFormat("{0}: {1}\r\n", v.Key, v.Value);
            }
            sb.Append("\r\n");
            sb.Append(ASCIIEncoding.ASCII.GetString(Content));
            return sb.ToString();
        }

        public override byte[] Serialize()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("HTTP/{0} {1} {2}\r\n", Version, StatusCode, StatusMessage);
            foreach (var v in Headers)
            {
                if (v.Value != null)
                    sb.AppendFormat("{0}: {1}\r\n", v.Key, v.Value);
            }
            sb.Append("\r\n");
            byte[] header = ASCIIEncoding.ASCII.GetBytes(sb.ToString());
            HttpHandler.Log(LogLevel.Info, String.Format("Server response : {0}{0}{1}", Environment.NewLine, ASCIIEncoding.ASCII.GetString(header)));
            if (!HasPayload)
                return header;

            byte[] response = new byte[header.Length + Content.Length];
            Array.Copy(header, 0, response, 0, header.Length);
            Array.Copy(Content, 0, response, header.Length, Content.Length);
#if(DEBUG)
            using (FileStream fs = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"response.gz", FileMode.OpenOrCreate))
            {
                fs.Write(Content, 0, Content.Length);
            }
           
            HttpHandler.Log(LogLevel.Info, Pixills.Tools.Log.Helpers.HexDump(Content));
            //HttpHandler.Log(LogLevel.Info, WriteBytesAsHex(RawContent));
#endif
            return response;
        }
    }
}
