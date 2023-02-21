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

        public async Task Start()
        {
            try
            {
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
