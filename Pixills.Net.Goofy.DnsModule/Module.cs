using System.IO;
using Pixills.Net.Goofy.Modules;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Pixills.Net.Goofy.DnsModule
{
    public class Module : IModule
    {
        public event Action<Pixills.Tools.Log.LogEventArgs> LogEvent;

        public string ModuleName
        {
            get { return string.Format("DNS Modue {0}", Assembly.GetExecutingAssembly().GetName().Version); }
        }

        public Task<byte[]> GetResponse(MemoryStream buffer)
        {
            return null;
        }
    }
}
