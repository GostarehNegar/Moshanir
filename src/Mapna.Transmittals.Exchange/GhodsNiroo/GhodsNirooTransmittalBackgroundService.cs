using GN.Library.SharePoint;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mapna.Transmittals.Exchange.GhodsNiroo.SharePoint;
using Mapna.Transmittals.Exchange.GhodsNiroo.Incoming;

namespace Mapna.Transmittals.Exchange.GhodsNiroo
{
    class GhodsNirooTransmittalBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly GhodsNirooTransmittalOptions options;
        private readonly ILogger<GhodsNirooTransmittalBackgroundService> logger;
        private readonly IGhodsNirooIncomingQueue queue;
        private readonly IClientContextFactory factory;

        public GhodsNirooTransmittalBackgroundService(IServiceProvider serviceProvider, GhodsNirooTransmittalOptions options, 
            ILogger<GhodsNirooTransmittalBackgroundService> logger, IGhodsNirooIncomingQueue queue, IClientContextFactory factory)
        {
            this.serviceProvider = serviceProvider;
            this.options = options;
            this.logger = logger;
            this.queue = queue;
            this.factory = factory;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () => {


                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        using (var srv = new GhodsNirooSharePointContext(this.factory.CreateContext(this.options.ConnectionString)))
                        {
                            foreach(var job in await srv.GetPendingJobs())
                            {
                                try
                                {
                                    var req = System.Text.Json.JsonSerializer.Deserialize<IncomingTransmittalRequest>(job.Content);
                                    queue.Enqueue(req);
                                    this.logger.LogInformation(
                                        $"Pending Job requeued. {job.Title}");
                                }
                                catch(Exception err)
                                {
                                    this.logger.LogError(
                                        $"An error occured while trying to enqueue pending jobs. Err:{err.GetBaseException().Message}");

                                }
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        this.logger.LogError(
                            $"An error occured while trying to enqueue pending jobs. Err:{err.GetBaseException().Message}");

                    }
                    await Task.Delay(3 * 60 * 1000);

                }

            });
        }
    }
}
