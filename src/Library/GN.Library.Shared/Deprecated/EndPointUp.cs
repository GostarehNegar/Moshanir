using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Contracts_Deprecated
{
    public class EndpointUp
    {
        public string Name { get; set; }
        public bool IsServer { get; set; }
    }
    public class EndPointBeat
    {
        public string Name { get; set; }
        public bool IsServer { get; set; }
    }
    public class EndpointDown
    {
        public string Name { get; set; }
    }
}
