using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixills.Net.Http
{
    public enum HandleSslStrategy : byte
    {
        Undefined = 0x00,
        Tunnel, ManInTheMiddle, SslStrip
    }
}
