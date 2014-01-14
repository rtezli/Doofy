using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixills.Tools.Log
{
    public interface ICanLog
    {
        event Action<LogEventArgs> Log;
    }
}
