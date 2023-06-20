using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.ServiceDiscovery
{
    /// <summary>
    /// Represents data about a service that is provided by a bode.
    /// 
    /// </summary>
    public class ServiceData
    {
        //public IDictionary<string, string> Headers { get; set; } = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// The name of this service, odten used to recognize it on the network
        /// </summary>
        public string Name { get; set; }
        public string Category { get; set; }
        public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public ulong StartedOn { get; set; }
        public ulong LastSeenOn { get; set; }
        public string Status { get; set; }
        public JobData[] Jobs { get; set; }
    }
    public class JobData
    {
        public string Name { get; set; }
        public string ServiceName { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        
        public ulong StartedOn { get; set; }
        public ulong LastSeenOn { get; set; }
        public string Status { get; set; }



    }
}
