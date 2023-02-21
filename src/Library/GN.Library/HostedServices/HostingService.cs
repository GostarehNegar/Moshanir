using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace GN.Library.HostedServices
{

    class HostingService : IHostedService
    {
        private List<IHostedServiceEx> services;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.services = AppHost.GetServices<IHostedServiceEx>().ToList();
            return Task.WhenAll(services.Select(x => x.StartAsync(cancellationToken)));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
			if (services!=null)
				return Task.WhenAll(services.Select(x => x.StopAsync(cancellationToken)));
			return Task.CompletedTask;
        }
    }
}
