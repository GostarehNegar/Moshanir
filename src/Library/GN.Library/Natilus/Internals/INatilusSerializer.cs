using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Natilus.Internals
{
    public interface INatilusSerializer
    {
        byte[] Serialize(object obj);
        T Deserialize<T>(byte[] data);
    }
}
