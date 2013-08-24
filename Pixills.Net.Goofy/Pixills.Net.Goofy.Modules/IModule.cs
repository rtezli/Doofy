using Pixills.Tools.Log;
using System;
using System.IO;

namespace Pixills.Net.Goofy.Modules
{
    public interface IModule
    {
        event Action<LogEventArgs> LogEvent;

        string ModuleName { get; }

        byte[] ProcessRequest(MemoryStream buffer);
    }
}
