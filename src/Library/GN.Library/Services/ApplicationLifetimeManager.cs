using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Services
{
    class ApplicationLifetimeManager : BackgroundService
    {
        public IHostApplicationLifetime lifetime;
        public IApplicationLifetime lifetime1;
        public ApplicationLifetimeManager(IServiceProvider provider)
        {
            this.lifetime = provider.GetServiceEx<IHostApplicationLifetime>();
            this.lifetime1 = provider.GetServiceEx<IApplicationLifetime>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var f = $"_EXIT_{Process.GetCurrentProcess().Id}";
            var fileName = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), f);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (File.Exists(fileName))
                    {
                        Console.WriteLine($"Exit Signal Detected. We will shutdown application...");
                        File.Delete(fileName);
                        this.lifetime?.StopApplication();
                        this.lifetime1?.StopApplication();
                        break;
                    }
                    await Task.Delay(10*1000);
                }
                catch (Exception err)
                {

                }
            }
            
        }
    }
}
