using System;
using System.Collections.Generic;
using System.Text;

namespace Mapna.Transmittals.Exchange.Internals
{

    [Flags]
    public enum StrategyFlags
    {
        Direct = 0,
        IgnoreCase = 1,
        DeclaredOnly = 2,
        Instance = 2^2,
        Static = 2^3,
        Public = 2^4,
    }
}
