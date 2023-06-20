using GN.Library.Messaging;
using GN.Library.ServiceDiscovery;
using GN.Library.Shared.ServiceDiscovery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.ServerManagement
{
    public interface IServiceController
    {

    }
    public class ServerProcessControler : BackgroundService, IServiceController
    {
        private List<ProcessWrapper> processes = new List<ProcessWrapper>();
        private IConfiguration Configuration;
        public ServerProcessControler(ILogger<ServerProcessControler> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.Configuration = serviceProvider.GetServiceEx<IConfiguration>();

        }
        private Process p;
        private readonly ILogger<ServerProcessControler> logger;
        private readonly IServiceProvider serviceProvider;
        public ProcessWrapper GetProcessById( int processId)
        {
            return this.processes
                .FirstOrDefault(x => x.Process != null && x.Process.Id == processId);
        }
        private async Task HandleNode()
        {
            try
            {
                await Task.CompletedTask;
                var nodeStatus = this.serviceProvider.GetServiceEx<IServiceDiscovery>().NodeStatus;
                foreach (var peer in nodeStatus.Peers.Values)
                {
                    if (int.TryParse(peer.ProcessId, out var _id))
                    {
                        var p = this.processes.FirstOrDefault(x => x.Process != null && x.Process.Id == _id);
                        if (p == null)
                        {
                            var process = Process.GetProcessById(_id);
                            this.processes.Add(new ProcessWrapper(process,
                                peer,
                                this.serviceProvider.GetServiceEx<ILoggerFactory>().CreateLogger<ProcessWrapper>(), 
                                this.Configuration));
                        }

                    }

                }
            }
            catch (Exception err)
            {

            }
        }

        public async Task Start()
        {
            try
            {
                await this.HandleNode();
                await this.serviceProvider.GetServiceEx<IMessageBus>()
                   .CreateSubscription()
                   .UseTopic(LibraryConstants.Subjects.ServiceDiscovery.HeartBeatEvent)
                   .UseHandler(ctx => this.HandleNode())
                   .Subscribe();

                this.processes = Configuration
                .GetSection("Services").Get<List<ServiceInfo>>()
                .Select(x => new ProcessWrapper(x, this.serviceProvider.GetServiceEx<ILoggerFactory>().CreateLogger<ProcessWrapper>(), this.Configuration))
                .ToList();
                await Task.WhenAll(this.processes.Select(x => x.Start()));
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to start some processes. {err.GetBaseException().Message}");

            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
            await this.Start();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.WhenAll(this.processes.Select(x => x.Visit()));
                }
                catch { }
                await Task.Delay(1 * 6 * 1000);
            }

        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            try
            {
                await Task.WhenAll(this.processes.Select(x => x.Stop()));
            }
            catch
            {
                this.logger.LogError("An error occured while trying to shutdown some process.");
            }
        }
    }
}
