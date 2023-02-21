using GN.Library.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library
{
	public interface IServiceDiscoveryEx : IHostedService
	{

	}
	class ServiceDiscoveryEx : IServiceDiscoveryEx
	{
		public class ServiceData
		{
			public string Name { get; set; }
			public string Url { get; set; }
			public bool IsMessageServer { get; set; }
		}
		private IPublicDbRepository<string, ServiceData> repo;
		private readonly ILogger logger;
		public ServiceDiscoveryEx(IPublicDbRepository<string, ServiceData> repository, ILogger<ServiceDiscoveryEx> logger)
		{
			this.repo = repository;
			this.logger = logger;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
