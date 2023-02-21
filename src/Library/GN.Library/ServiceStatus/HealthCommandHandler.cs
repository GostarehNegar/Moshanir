using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GN.Library.Messaging;
using GN.Library.Messaging.Internals;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.Extensions.Logging;
using GN.Library.Shared.Internals;

namespace GN.Library.ServiceStatus
{
    class HealthCommandHandler
	{
		private readonly ILogger<HealthCommandHandler> logger;
		private readonly IServiceProvider serviceProvider;

		public HealthCommandHandler(ILogger<HealthCommandHandler> logger, IServiceProvider serviceProvider)
		{
			this.logger = logger;
			this.serviceProvider = serviceProvider;
		}
		public async Task Handle(IMessageContext<HealthCommand> ctx)
		{
			var log = new StringBuilder();
			try
			{
				var repo = await this.serviceProvider.GetServiceEx<ServiceStatusTask>().CreateReport(default);
				log.Append(repo.Report.ToString());

			}
			catch (Exception err)
			{

			}
			await ctx.Reply(new
			{
				Message = "",
				Log = log.ToString()
			}); ;
		}
	}
}
