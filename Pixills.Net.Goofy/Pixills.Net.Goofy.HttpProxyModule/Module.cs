using Pixills.Net.Goofy.Modules;
using Pixills.Net.Http;
using System;
using System.Collections.Generic;
using System.IO;
using Pixills.Tools.Log;

namespace Pixills.Net.Goofy.HttpProxyModule
{
    public class Module : IModule
    {
        public List<Action> RequestFilterActions;
        public List<Action> ResponseFilterActions;
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

        public byte[] ProcessRequest(MemoryStream ms)
        {
            byte[] responseData = new byte[0];

            var h = new HttpHandler(Log);

            var request = new HttpRequest();
            if (!request.TryParseProxyRequest(ms))
                return responseData;

            RequestFilterActions.ForEach(f => f.Invoke());
            
            var response = request.GetResponse();

            RequestFilterActions.ForEach(f => f.Invoke());

            return response.ResponseStream.GetBuffer();
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
