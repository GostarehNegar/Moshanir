using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mapna.Transmittals.Exchange.Internals;
using System.Linq;
using Mapna.Transmittals.Exchange.Services.Queues.Incomming;
using Mapna.Transmittals.Exchange.Domain.Outgoing;

namespace Mapna.Transmittals.Exchange.Services
{
    class JobHostedService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<JobHostedService> logger;

        public JobHostedService(IServiceProvider serviceProvider, ILogger<JobHostedService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var repo = scope.ServiceProvider.GetService<ITransmittalRepository>();
                        var queue = scope.ServiceProvider.GetService<IIncommingQueue>();
                        var outgoing = scope.ServiceProvider.GetService<IOutgoingQueue>();
                        var pendings = (await repo.GetPendingJobs())
                               .Where(x => x.Direction == "In").ToArray();
                        pendings
                            .Where(x=>x.Direction=="In")
                            .Select(x => x.GetTransmittal())
                            .Where(x => x != null)
                            .Select(x => queue.Enqueue(x))
                            .ToArray();
                        this.logger.LogInformation(
                            $"{pendings.Length} Pending Jobs Requeued.");
                    }
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var repo = scope.ServiceProvider.GetService<ITransmittalRepository>();
                        var queue = scope.ServiceProvider.GetService<IIncommingQueue>();
                        var outgoing = scope.ServiceProvider.GetService<IOutgoingQueue>();
                        var pendings = (await repo.GetPendingJobs()).Where(x => x.Direction == "Out").ToArray();
                        pendings
                            .Where(x => x.Direction == "Out")
                            .Select(x => outgoing.Enqueue(x.InternalId))
                            .ToArray();
                        this.logger.LogInformation(
                            $"{pendings.Length} Pening Outgoing Jobs Requeued.");
                    }
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var repo = scope.ServiceProvider.GetService<ITransmittalRepository>();

                        var waitings = await repo.GetWaitingTransmittals();
                        var outgoing = scope.ServiceProvider.GetService<IOutgoingQueue>();

                        waitings.Select(x => outgoing.Enqueue(x.TransmittalNo))
                            .ToArray();
                        this.logger.LogInformation(
                            $"{waitings.Length} Waiting Jobs Enqueued.");
                    }

                }
                catch (Exception err)
                {
                    this.logger.LogError(
                        $"An error occured while trying to watch Job list. {err.GetBaseException().Message}");
                }
                await Task.Delay(60 * 1000, stoppingToken);

            }
        }
    }
}
