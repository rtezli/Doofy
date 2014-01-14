using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixills.Tools.Log
{
    public class LogEventArgs : EventArgs
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; }

        public LogEventArgs()
        {

        }

        public LogEventArgs(LogLevel l, string s)
        {
            Level = l;
            Message = LogHelpers.EscapeNonPrintable(s);
        }

    }
}
