using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixills.Net.Goofy.HttpProxyModule.Filters
{
    public interface IFilter
    {
        byte[] Apply(byte[] b);
    }
}
