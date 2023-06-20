using GN.Library.Messaging;
using GN.Library.Shared.ServiceDiscovery;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.ServiceDiscovery
{
    class NodeService : BackgroundService, IServiceDiscovery
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger logger;
        private readonly IMessageBus bus;
        private NodeStatus status;
        private int HeartBitRate;

        public NodeService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.logger = this.serviceProvider.GetServiceEx<ILogger<NodeService>>();
            this.bus = this.serviceProvider.GetServiceEx<IMessageBus>();
            this.HeartBitRate = this.bus.Advanced().Options.HeartBit;


        }
        public NodeStatusData NodeStatus => GetStatus().Status;
        private NodeStatus GetStatus()
        {
            if (this.status == null)
            {
                this.status = new NodeStatus(this.serviceProvider);
                lock (this.status)
                {
                    //this.status.Initialize();
                    this.status.Status.Node.Name = this.bus.Advanced().EndpointName;

                }
            }
            return this.status;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var state = this.GetStatus().Status;
                    //this.logger.LogInformation(
                    //    $"Status: {this.GetStatus().Status}");
                    await this.bus.CreateMessage(GetStatus().Status)
                        .UseTopic(LibraryConstants.Subjects.ServiceDiscovery.HeartBeatEvent)
                        .Publish();
                    await Task.Delay(this.HeartBitRate * 1000, stoppingToken);
                };
            });
        }
        private async Task GetStatus(IMessageContext context)
        {
            await Task.CompletedTask;
            //var message = context.Cast<NodeStatusData>()?.Message?.Body;

            await this.bus.CreateMessage(GetStatus().Status)
                        .UseTopic(LibraryConstants.Subjects.ServiceDiscovery.HeartBeatEvent)
                        .Publish();
            

        }
        private async Task HandleHeartBeat(IMessageContext context)
        {
            await Task.CompletedTask;
            var message = context.Cast<NodeStatusData>()?.Message?.Body;
            if (message != null)
            {
                this.GetStatus().Handle(message, context.Message.From());
            }

        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.bus.CreateSubscription()
                .UseTopic(LibraryConstants.Subjects.ServiceDiscovery.HeartBeatEvent)
                .UseHandler(this.HandleHeartBeat)
                .Subscribe();

            await this.bus.CreateSubscription()
                .UseTopic(LibraryConstants.Subjects.ServiceDiscovery.GetStatus)
                .UseHandler(this.GetStatus)
                .Subscribe();



            await base.StartAsync(cancellationToken);
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {

            await base.StopAsync(cancellationToken);
        }

        public IEnumerable<ServiceData> GetServices()
        {
            return this.GetStatus().GetServices();
        }

        public Task PubStatus()
        {
            return this.bus.CreateMessage(GetStatus().Status)
                       .UseTopic(LibraryConstants.Subjects.ServiceDiscovery.HeartBeatEvent)
                       .Publish();
        }
    }
}
