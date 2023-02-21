using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Natilus.Internals
{
    public class NatilusOptions
    {
        internal INatilusSerializer GetSerializer()
        {
            return NatilusSerializer.Default;
        }
    }
}
