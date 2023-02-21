using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.MicroServices
{
	public static class MicroServicesExtensions
	{
		
		public static IServiceCollection AddMicroServices<T>(this IServiceCollection services, IConfiguration configuration, Action<MicroServicesConfiguration> configure)
			where T : class, IMicroService
		{

			services.AddSingleton<T>();
			services.AddSingleton<IHostedService>(s => s.GetService<T>());


			return services;

		}
		/// <summary>
		/// Adds microsevices supports. Use 'configure' to configure the microservices to be added.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="services"></param>
		/// <param name="configuration"></param>
		/// <param name="configure"></param>
		/// <returns></returns>
		public static IServiceCollection AddMicroServicesSupport(this IServiceCollection services, IConfiguration configuration, Action<MicroServicesConfiguration> configure)
		{
			var options = MicroServicesConfiguration.Current;
			configuration.Bind("MicroServices", options);
			if (!Uri.IsWellFormedUriString(options.GatewayServerUrl, UriKind.Absolute))
			{
				options.GatewayServerUrl = configuration["ServerUrl"];
			}
			if (!Uri.IsWellFormedUriString(options.GatewayServerUrl, UriKind.Absolute))
			{
				options.GatewayServerUrl = "http://localhost:2352";
			}
			configure?.Invoke(options);
			services.AddSingleton<MicroServicesConfiguration>(options);
			services.AddSingleton<MicroServiceService>();
			services.AddSingleton<IHostedService>(s => s.GetService<MicroServiceService>());
			return services;
		}
		public static IApplicationBuilder UseMicroServicesSupport(this IApplicationBuilder applicationBuilder)
		{

			return applicationBuilder;

		}
	}
}
