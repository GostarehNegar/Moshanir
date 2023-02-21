using GN.Library.TaskScheduling;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.ServiceStatus
{
	class HealthCheckReport
	{
		public StringBuilder Report { get; } = new StringBuilder();

		public HealthCheckReport()
		{

		}
	}
	class ServiceStatusTask : ScheduledTask
	{
		private readonly ILogger logger;
		public override string Schedule => CronHelper.Every5Minutes;
		private int count = 0;

		public ServiceStatusTask(ILogger<ServiceStatusTask> logger)
		{
			this.logger = logger;

		}
		public override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			if (1 == 0)
			{
				using (var ctx = AppHost.Context.Push())
				{
					logger.LogInformation("HealthCareTask Starts...");
					var context = new StatusReportContext(ctx.AppServices);
					foreach (var item in AppHost.GetServices<IHealthCheck>().ToList())
					{
						var conetxt = new HealthCheckContext();
						var result = await item.CheckHealthAsync(new HealthCheckContext(), cancellationToken);
						switch (result.Status)
						{
							case HealthStatus.Healthy:
								this.logger.LogInformation($"{result.Description} Status:{result.Status} \r\n\t{result.GetReport()}");
								break;
							case HealthStatus.Unhealthy:
								this.logger.LogWarning($"{result.Description} Status:{result.Status} \r\n\t{result.GetReport()}");
								break;
							case HealthStatus.Degraded:
								break;
						}
						//this.logger.LogInformation($"{result.Description} Status:{result.Status} \r\n\t{result.GetReport()}");
					}
					//foreach (var item in AppHost.GetServices<IServiceStatusReporter>().ToList())
					//{
					//	if (cancellationToken.IsCancellationRequested)
					//		break;
					//	try
					//	{
					//		context.InfoFormat(item.GetType().Name);
					//		item.GenerateStatusReport(context);
					//		context.InfoFormat("=====================================\r\n");
					//	}
					//	catch (Exception err)
					//	{
					//		logger.LogError(
					//			"An error occured while trying to execute this Health Service: {0}, Err: {1}", item.GetType(), err.Message);
					//	}
					//}
					logger.LogInformation("\r\n{0}\r\n", context.Writer.ToString());
					await Task.FromResult(true).ConfigureAwait(false);
					logger.LogInformation("HealthCareTask Exceuted.");

				}
			}
			else
			{
				try
				{
					await Task.Delay(2 * 1000);
					var repo = await this.CreateReport(cancellationToken);
					this.logger.LogInformation(repo.Report.ToString());
					
				}
				catch (Exception err)
				{

				}
			}
		}

		public async Task<HealthCheckReport> CreateReport(CancellationToken cancellationToken)
		{
			var report = new HealthCheckReport();
			var log = report.Report;
			log.AppendLine();
			log.AppendLine("*************************************");
			log.AppendLine("Health Check Report:");
			try
			{
				using (var context = AppHost.Context.Push())
				//using (var context = AppHost.Context)
				{
					foreach (var item in AppHost.GetServices<IHealthCheck>().ToList())
					{
						try
						{
							var result = await item.CheckHealthAsync(new HealthCheckContext(), default)
								.TimeOutAfter(10 * 1000, cancellationToken, throwIfTimeOut: true);
							log.AppendLine("----");
							switch (result.Status)
							{
								case HealthStatus.Healthy:
									//this.logger.LogInformation($"{result.Description} Status:{result.Status} \r\n\t{result.GetReport()}");
									
									log.AppendLine($"{result.Description} Status:{result.Status} \r\n{result.GetReport()}");
									break;
								case HealthStatus.Unhealthy:
									//this.logger.LogWarning($"{result.Description} Status:{result.Status} \r\n{result.GetReport()}");
									log.AppendLine($"{result.Description} Status:{result.Status} \r\n\t{result.GetReport()}");
									break;
								case HealthStatus.Degraded:
									break;
							}
						}
						catch (Exception err)
						{
							log.AppendLine($"An error occured while trying to execute a HealthCheck item. Item:{item.GetType().Name}, Error:{err.GetBaseException().Message}");
						}
						//this.logger.LogInformation($"{result.Description} Status:{result.Status} \r\n\t{result.GetReport()}");
					}
				}

				log.AppendLine("*************************************");
			}
			catch (Exception err)
			{
				throw;
			}
			return report;
		}
	}
}
