using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pixills.Tools.Log;

namespace Pixills.Net.Http
{
    public class HttpHandler
    {
        private static Action<LogEventArgs> LogAction { get; set; }

        public HttpHandler()
        {

        }

        public HttpHandler(Action<LogEventArgs> a)
        {
            LogAction = a;
        }

        internal static void Log(LogLevel l, string s)
        {
            LogAction.Invoke(new LogEventArgs(l, s));
        }
    }
}
