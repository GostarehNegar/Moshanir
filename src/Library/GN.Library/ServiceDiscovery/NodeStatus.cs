using GN.Library.Messaging;
using GN.Library.Shared.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace GN.Library.ServiceDiscovery
{
    class NodeStatus
    {
        private readonly IServiceProvider serviceProvider;

        public NodeStatusData Status { get; private set; }
        public NodeStatus(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.Status = new NodeStatusData();
            lock (this.Status)
            {
                this.Status.Node = this.GetNodeData();
                this.Status.Peers = new Dictionary<string, NodeData>();

            }

        }
        private NodeData GetNodeData()
        {
            var result = new NodeData();
            var process = Process.GetCurrentProcess();
            result.ProcessId = process.Id.ToString();
            result.Headers = new Dictionary<string, string>();
            result.MachineName = process.MachineName;
            result.Argv = Environment.GetCommandLineArgs();
            result.MachineName = Environment.MachineName;
            result.Name = this.serviceProvider.GetServiceEx<IMessageBus>().Advanced().EndpointName;
            result.Services = this.serviceProvider.GetServices<IServiceDataProvider>()
                .Select(x => x.GetData())
                .ToArray();
            
            //.ToDictionary(x => x.Name);
            return result;
        }
        public IEnumerable<ServiceData> GetServices()
        {
            this.Status.Peers = this.Status.Peers ?? new Dictionary<string, NodeData>();
            var result = (this.Status.Node.Services ?? new ServiceData[] { })
                .ToList();
            foreach(var p in this.Status.Peers.Values)
            {
                result.AddRange(p.Services ?? new ServiceData[] { });
            }
            return result;
           
        }
        public void Handle(NodeStatusData data, string endpoint)
        {

            if (data != null && data.Node?.Name != this.Status.Node.Name)
            {
                lock (this.Status)
                {
                    this.Status.Peers = this.Status.Peers ?? new Dictionary<string, NodeData>();
                    if (!string.IsNullOrWhiteSpace(data?.Node?.Name))
                    {
                        data.Node.LastSeen = DateTime.UtcNow.Ticks;
                        data.Node.Endpoint = endpoint;
                        this.Status.Peers[data.Node.Name] = data.Node;
                        foreach (var item in data.Peers ?? new Dictionary<string, NodeData>())
                        {
                            if (item.Key != this.Status.Node.Name && !this.Status.Peers.ContainsKey(item.Key))
                            {
                                this.Status.Peers[item.Key] = item.Value;
                            }
                        }
                    }
                }

            }

        }
    }
}
