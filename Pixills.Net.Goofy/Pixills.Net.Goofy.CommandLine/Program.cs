using Pixills.Tools.Log;
using Pixills.Net.Goofy.Modules;
using System;
using System.Diagnostics;
using System.Linq;

namespace Pixills.Net.Goofy.CommandLine
{
    class Program
    {
        private static int _minLogLevel = 2;

        private static void Main(string[] args)
        {
            if (args.Any() && !string.IsNullOrEmpty(args[0]))
            {
                if (args[0].ToLower() == "-v")
                    _minLogLevel = 1;
                if (args[0].ToLower() == "-vv")
                    _minLogLevel = 0;
            }

            var httpProxy = new Server();
            httpProxy.LogEvent += LogToConsole;
            try
            {
                httpProxy.Start(8080, 200);
            }
            catch (Exception e)
            {
                LogToConsole(new LogEventArgs(LogLevel.Error, "Could not start server. Reason : " + e.Message));
            }
        }

        private static void LogToConsole(LogEventArgs e)
        {
            ConsoleColor c = Console.ForegroundColor;
            var d = DateTime.Now;

            var logString = String.Format("[{0:d} - {0:t}] [{1}] {2}", d, e.Level.ToString().ToUpper(), e.Message);
            Debug.WriteLine(logString);

            if ((int)e.Level >= _minLogLevel)
            {
                switch (e.Level)
                {
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                }
                Console.WriteLine(logString);
            }

           Console.ForegroundColor = c;
        }
    }
}
