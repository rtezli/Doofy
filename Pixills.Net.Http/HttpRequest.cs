using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Pixills.Tools.Log;
using System.Net.Sockets;
using System.Text;

namespace Pixills.Net.Http
{
    public class HttpRequest : HttpMessage
    {
        public string Domain { get; set; }
        public string Host { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public string Resource { get; set; }
        public int TargetPort { get; set; }
        public MemoryStream RequestStream { get; set; }

        public HttpRequest()
            : base()
        {
            //defaults
            TargetPort = 80;
        }

        public HttpRequest(string s)
            : base()
        {
            //defaults
            TargetPort = 80;
            Url = s;
        }

        public bool TryParseRequest(MemoryStream m)
        {
            RequestStream = m;
            RawContent = m.GetBuffer();
            var p = UTF8Encoding.UTF8.GetString(RawContent);

            string[] HeaderAndData = p.Split(new String[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                var headerFields = HeaderAndData[0].Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                var MethodResourceversion = headerFields[0];
                var MethodResourceversionSplitted = MethodResourceversion.Split(' ');

                Method = MethodResourceversionSplitted[0];
                Resource = MethodResourceversionSplitted[1];
                Version = Version.Parse(MethodResourceversionSplitted[2].ToLower().Replace("http/", ""));

                for (int i = 1; i < headerFields.Length; i++)
                {
                    var PropertyValue = headerFields[i].Split(new String[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

                    var key = PropertyValue[0].Trim().ToLower();
                    var value = PropertyValue[1].Trim();

                    if (Headers.Any(v => v.Key.ToLower().Equals(key)) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        Headers[key] = value;

                    if (!Headers.ContainsKey(key))
                        HttpHandler.Log(LogLevel.Warning, "Key \"" + key + "\" is missing in header list");
                }

                if (!string.IsNullOrEmpty(Headers["Content-Type"]))
                    ContentType = Headers["Content-Type"];
                if (!string.IsNullOrEmpty(Headers["Content-Length"]))
                    ContentLength = Convert.ToInt32(Headers["Content-Length"]);

                if (!string.IsNullOrEmpty(Headers["Cookie"]))
                {
                    if (Cookies == null)
                        Cookies = new CookieContainer();

                    var cookies = Headers["Cookie"].Split(';');
                    foreach (var cookie in cookies)
                    {
                        var keyValue = cookie.Trim().Split(':');
                        if (keyValue.Length >= 2)
                        {
                            Cookies.Add(new Cookie()
                            {
                                Name = keyValue[0],
                                Value = keyValue[1],
                                Domain = Domain
                            });
                        }
                    }
                }


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

        public bool TryParseProxyRequest(MemoryStream m)
        {
            RequestStream = m;
            RawContent = m.GetBuffer();
            var p = UTF8Encoding.UTF8.GetString(RawContent);

            string[] HeaderAndData = p.Split(new String[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                var HeaderFields = HeaderAndData[0].Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                var MethodResourceversion = HeaderFields[0];
                var MethodResourceversionSplitted = MethodResourceversion.Split(' ');

                Method = MethodResourceversionSplitted[0];
                Resource = MethodResourceversionSplitted[1];
                Url = Resource;
                this.Version = Version.Parse(MethodResourceversionSplitted[2].ToLower().Replace("http/", ""));

                for (int i = 1; i < HeaderFields.Length; i++)
                {
                    var PropertyValue = HeaderFields[i].Split(new String[] { ": " }, StringSplitOptions.None);

                    var key = PropertyValue[0].Trim();

                    if (PropertyValue.Length == 2)
                    {
                        var value = PropertyValue[1].Trim();

                        if (Headers.Any(v => v.Key.ToLower().Equals(key.ToLower())) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        {
                            if (key == "Proxy-Connection")
                            {
                                Headers["Connection"] = value;
                            }

                            else if (key == "UA-CPU")
                            {
                                Headers["UA-CPU"] = value;
                            }
                            else
                            {
                                Headers[key] = value;
                            }
                        }
                    }
                    else
                        HttpHandler.Log(LogLevel.Warning, "Empty key in header : \"" + key);

                    if (!Headers.ContainsKey(key))
                        HttpHandler.Log(LogLevel.Warning, "Key \"" + key + "\" is missing in header list");

                }

                if (!string.IsNullOrEmpty(Headers["Content-Type"]))
                    ContentType = Headers["Content-Type"];
                if (!string.IsNullOrEmpty(Headers["Content-Length"]))
                    ContentLength = Convert.ToInt32(Headers["Content-Length"]);

                if (Headers["Host"] == null)
                {
                    var breakupresource = this.Resource.Split('/');
                    Headers["Host"] = breakupresource[2];
                }
                if (Regex.IsMatch(this.Resource, "http://.*/"))
                {
                    var hostPart = "http://" + Headers["Host"];
                    Resource = this.Resource.Replace(hostPart, "");
                }
                if (HeaderAndData.Length >= 2)
                {
                    HasPayload = true;
                    var contentLength = RawContent.Length - HeaderAndData[0].Length - 4;
                    Content = new byte[contentLength];
                    Array.Copy(RawContent, RawContent.Length - contentLength, Content, 0, contentLength);
                }
                this.Host = Headers["Host"];
                return true;

            }
            catch (Exception e)
            {
                HttpHandler.Log(LogLevel.Error, "Http request parsing error : " + e.Message);
                return false;
            }

        }

        public static HttpRequest Create(String url)
        {
            HttpRequest r = new HttpRequest();
            r.Url = url;
            return r;
        }

        public HttpResponse GetResponse()
        {
            HttpResponse r = new HttpResponse();
            try
            {
                using (TcpClient c = new TcpClient())
                {
                    c.Connect(Host, TargetPort);

                    using (NetworkStream s = c.GetStream())
                    {
                        byte[] data = Serialize();
                        s.Write(data, 0, data.Length);

                        // Switching over. Reusing the request stream as response stream
                        RequestStream = new MemoryStream();
                        r.ResponseStream = RequestStream;

                        byte[] inBuffer = new byte[1024];
                        int bytes = 0;
                        while ((bytes = s.Read(inBuffer, 0, inBuffer.Length)) != 0)
                        {
                            HttpHandler.Log(LogLevel.Info, "Received : " + bytes + " bytes");
                            r.ResponseStream.Write(inBuffer, 0, bytes);
                            if (bytes < inBuffer.Length)
                                break;
                        }

                        if (bytes > 0)
                        {
                            r.RawContent = r.ResponseStream.GetBuffer();
                            var result = r.TryParseResponse(r.ResponseStream);
                        }
                    }
                }
                return r;
            }
            catch (SocketException e)
            {
                throw e;
            }

        }

        public HttpWebResponse GetResponseWithApi()
        {
            HttpWebResponse response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.UserAgent = Headers["User-Agent"];

                if (Headers["Cookie"] != null)
                {
                    request.Headers["Cookie"] = Headers["Cookie"];
                }
                if (Headers["Accept-Encoding"] != null)
                {
                    request.Headers["Accept-Encoding"] = Headers["Accept-Encoding"];
                }
                if (HasPayload)
                {
                    request.Method = "POST";
                    request.ContentType = ContentType;
                    request.ContentLength = Content.Length;
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(Content, 0, Content.Length);
                    }
                }
                response = (HttpWebResponse)request.GetResponse();
            }

            catch (WebException ex)
            {
                var resStream = ex.Response.GetResponseStream();
                return (HttpWebResponse)ex.Response;
            }

            catch (Exception e)
            {
                throw e;
            }
            return response;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1} HTTP/{2}\r\n", Method, Resource, Version);
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
            sb.AppendFormat("{0} {1} HTTP/{2}\r\n", Method, Resource, Version);
            foreach (var v in Headers)
            {
                if (v.Value != null)
                    sb.AppendFormat("{0}: {1}\r\n", v.Key, v.Value);
            }
            sb.Append("\r\n");
            byte[] header = ASCIIEncoding.ASCII.GetBytes(sb.ToString());
            HttpHandler.Log(LogLevel.Info, String.Format("Client request : {0}{0}{1}", Environment.NewLine, ASCIIEncoding.ASCII.GetString(header)));
            if (!HasPayload)
                return header;

            byte[] request = new byte[header.Length + ContentLength];
            Array.Copy(header, 0, request, 0, header.Length);
            Array.Copy(Content, 0, request, header.Length - 1, request.Length);
            return request;
        }
    }
}
