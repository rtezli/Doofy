using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Pixills.Tools.Log;

namespace Pixills.Net.Http
{
    public abstract class HttpMessage
    {
        public CookieContainer Cookies { get; set; }
        public bool IsEncoded { get; set; }
        public bool HasPayload { get; set; }
        public Version Version { get; set; }

        public string ContentType { get; set; }
        public int ContentLength { get; set; }

        public byte[] _content;
        public byte[] RawContent;
        public byte[] Content 
        {
            get { return _content; }
            set 
            {
                _content = value;
                //var length = value.Sum(s => value.Length);
               // ContentLength = length;
            }
        }

        public MessageDirection Direction { get; set; }

        public Dictionary<string, string> Headers = new Dictionary<string, string>()
        {
                {"Accept",null},
                {"Accept-Language",null},
                {"Age", null},
                {"Accept-Encoding",null},
                {"Connection",null},
                {"Accept-Charset",null},
                {"Cache-Control",null},
                {"Content-Encoding",null},
                {"Content-Type",null},
                {"Content-Length",null},
                {"Content-Disposition",null},
                {"Cookie",null},
                {"Date",null},
                {"Expect",null},
                {"Expires",null},
                {"From",null},
                {"Host",null},
                {"If-Modified-Since",null},
                {"If-None-Match",null},
                {"If-Range",null},
                {"If-Unmodified-Since",null},
                {"Max-Forwards",null},
                {"Last-Modified",null},
                {"Location",null},
                {"P3P", null},
                {"Pragma",null},                
                {"Proxy-Authorization", null},
                {"Proxy-Connection",null},
                {"Range",null},
                {"Referer",null},
                {"Server",null},
                {"Set-Cookie",null},
                {"TE", null},
                {"Trailer",null},
                {"Transfer-Encoding",null},
                {"UA-CPU",null},
                {"Upgrade",null},
                {"User-Agent",null},
                {"Vary", null},
                {"Via",null},
                {"Warning",null},
                {"X-Cache",null},
                {"X-Content-Type-Options",null},
                {"X-Amz-Id-1", null},
                {"X-Amz-Id-2", null},
                {"X-Amz-Cf-Id", null},
                {"X-Xss-Protection",null},
                {"X-Frame-Options",null}
        };

        public HttpMessage()
        {
            Content = new byte[0];
        }

        public void Read(string s)
        {
            var message = s.Split(new string[]{"\r\n\r\n"}, StringSplitOptions.RemoveEmptyEntries);

            if (message.Length <= 0)
                throw new FormatException("Invalid HTTP message");

            ReadHeader(message[0].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));

            if (message.Length >= 2)
            {
                HasPayload = true;
                var data = new string[message.Length - 1];
                Array.Copy(message, 1, data, 0, data.Length);
                ReadData(data);
            }

        }

        public void ReadHeader(string[] data)
        {

        }

        public void ReadData(string[] data)
        {

        }

        public abstract byte[] Serialize();
    }
}
