using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.ServiceDiscovery
{

    /// <summary>
    /// Data about a node.
    /// A node is a process that connects to natilus network to provide
    /// some services.
    /// </summary>
    public class NodeData
    {
        public IDictionary<string,string> Headers { get; set; }
        /// <summary>
        /// The name of this node. This should be some unique name, that will be used to address
        /// this node in the network.
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The 'process id' last associated with this Node.
        /// </summary>
        public string ProcessId { get; set; }
      
        /// <summary>
        /// MachineName this node was last seen on.
        /// </summary>
        public string MachineName { get; set; }

        public string [] Argv { get; set; }

        /// <summary>
        /// Path on the machine to this node.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Script that can be used to start this node.
        /// </summary>
        public string StartScript { get; set; }

        public ServiceData[] Services { get; set; }

        public long LastSeen { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }
        //public IDictionary<string,NodeData> Peers { get; set; }
    }
}
