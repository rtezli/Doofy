using Pixills.Net.Goofy.Modules;
using Pixills.Net.Http;
using Pixills.Tools.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Pixills.Net.Goofy.HttpProxyModule
{
    public class Module : IModule
    {
        public List<Func<HttpRequest, HttpRequest>> RequestFilters;
        public List<Func<HttpResponse, HttpResponse>> ResponseFilters;

        public event Action<LogEventArgs> LogEvent;

        private readonly string _moduleName;

        public string ModuleName
        {
            get { return _moduleName; }
        }

        public Module()
        {
            _moduleName = "HttpProxy";
        }

        public Task<byte[]> GetResponse(MemoryStream ms)
        {
            var responseData = new byte[0];

            var h = new HttpHandler(Log);

            var request = new HttpRequest();
            if (!request.TryParseProxyRequest(ms))
                return new Task<byte[]>(() => responseData);

            foreach (var function in RequestFilters)
            {
                request = function.Invoke(request);
            }
            var response = request.GetResponse();
            foreach (var function in ResponseFilters)
            {
                response = function.Invoke(response);
            }

            return new Task<byte[]>(() => response.ResponseStream.GetBuffer());
        }

        public void Log(LogEventArgs e)
        {
            Log(e.Level, e.Message);
        }

        public void Log(LogLevel l, string s)
        {
            if (LogEvent != null)
                LogEvent(new LogEventArgs(l, s));
        }
    }
}
