using System.Collections.Generic;

namespace GN.Library.Shared.ServiceDiscovery
{
    public class NodeStatusData
    {
        public NodeData Node { get; set; }
        public IDictionary<string, NodeData> Peers { get; set; }

        public override string ToString()
        {
            return $"{Node} , Peers:{Peers?.Count}";
        }
    }
}
