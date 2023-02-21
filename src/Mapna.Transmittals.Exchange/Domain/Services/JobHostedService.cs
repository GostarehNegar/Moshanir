﻿using Microsoft.Extensions.Hosting;
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
                        var pendings = await repo.GetPendingJobs();
                        pendings
                            .Select(x => x.GetTransmittal())
                            .Where(x => x != null)
                            .Select(x => queue.Enqueue(x))
                            .ToArray();
                        this.logger.LogInformation(
                            $"{pendings.Length} Pending Jobs Requeued.");

                    }
                }
                catch (Exception err)
                {
                    this.logger.LogError(
                        $"An error occured while trying to watch Job list. {err.GetBaseException().Message}");
                }
                await Task.Delay(1 * 60 * 1000, stoppingToken);

            }
        }
    }
}