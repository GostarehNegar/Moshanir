using GN.Library.Contracts.Gateway;
using GN.Library.TaskScheduling;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.MicroServices
{
	public interface IMicroServiceService : IHostedService
	{

	}


	class MicroServiceService : HostedService, IMicroServiceService
	{
		private readonly MicroServicesConfiguration options;
		private readonly ILogger logger;
		private HttpClient httpClient;
		public MicroServiceService(MicroServicesConfiguration options, ILogger<MicroServiceService> logger)
		{
			this.options = options;
			this.logger = logger;
			//IHttpClientFactory factory;
			//this.httpClient = HttpClientFactory.Create();
		}
		public HttpClient GetClient()
		{
			if (this.httpClient == null)
			{
				this.httpClient = new HttpClient();// HttpClientFactory.Create();
			}
			return this.httpClient;
		}
		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			await base.StartAsync(cancellationToken);
		}
		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			var client = this.GetClient();
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(5 * 1000);
				try
				{
					var requestUrl = this.options.GatewayServerUrl + "/api/gateway";
					var model =
					new AddMicroServiceRequest
					{
						Urls = this.options.GetUrl()
					};

					var stringContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
					var response = await client.PutAsync(requestUrl, stringContent, cancellationToken);
					//var response = await httpClient.PutAsync<AddMicroServiceRequest>(requestUrl,
					//	new AddMicroServiceRequest
					//	{
					//		Urls = this.options.GetUrl()
					//	}
					//	, new System.Net.Http.Formatting.JsonMediaTypeFormatter());
					if (response.IsSuccessStatusCode)
					{

					}

				}
				catch (Exception err)
				{
					this.logger.LogError(
						"Connection to ApiGateway Failed. Url:{0}, Error:{1}", this.options.GatewayServerUrl, err.GetBaseException().Message);
				}
				await Task.Delay(this.options.Puase * 1000, cancellationToken);
			}
		}
		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await base.StopAsync(cancellationToken);
		}
		protected virtual Task OnStartAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}

}
