using Mapna.Transmittals.Exchange.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Mapna.Transmittals.Exchange.Services;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Hosting;
using Mapna.Transmittals.Exchange.Services.Queues.Incomming;

[assembly: InternalsVisibleTo("Mapna.Transmittals.Exchange.Tests")]
namespace Mapna.Transmittals.Exchange
{

    public static partial class MapnaTransmittalsExtensions
    {
        public static IServiceCollection AddTransmittalsExchange(this IServiceCollection services,
            IConfiguration configuration, Action<TransmittalsExchangeOptions> configure = null)
        {
            //var options = new TransmittalsExchangeOptions();
            var options = configuration
               .GetSection("ExchangeOptions").Get<TransmittalsExchangeOptions>() ?? new TransmittalsExchangeOptions();
            options.ConnectionString = options.ConnectionString ?? configuration?.GetConnectionString("wss");

            configure?.Invoke(options);
            services.AddSingleton(options);
            services.AddScoped<ITransmittalRepository, TransmittalRepository>();
            services.AddScoped<ITransmittalService, TransmitallService>();
            //services.AddSingleton<BackgroundBlockingCollectionWithContext>();
            services.AddSingleton<FileDownloadQueue>();
            services.AddSingleton<IFileDownloadQueue>(sp => sp.GetService<FileDownloadQueue>());
            services.AddSingleton<IHostedService>(sp => sp.GetService<FileDownloadQueue>());
            services.AddSingleton<TransmitallIncomingQueue>();
            services.AddSingleton<IIncommingQueue>(sp => sp.GetService<TransmitallIncomingQueue>());
            services.AddSingleton<IHostedService>(sp => sp.GetService<TransmitallIncomingQueue>());
            services.AddSingleton<JobHostedService>();
            services.AddSingleton<IHostedService>(sp => sp.GetService<JobHostedService>());
            return services;

        }
        public static bool IsRetryable(this Exception exception)
        {
            if (exception is TransmitalException exp)
            {
                return exp.IsRecoverable;
            }
            return true;
        }
        public static bool TryValidate(this TransmittalSubmitModel transmittal, out Exception message)
        {
            message = null;
            try
            {
                transmittal.Validate();
                return true;
            }
            catch (Exception err)
            {
                message = err.GetBaseException();

            }
            return false;
        }

        internal static string Serialize(object item)
        {
            return System.Text.Json.JsonSerializer.Serialize(item);
        }
        internal static bool TryDeserialize<T>(string text, out T result)
        {
            result = default(T);
            try
            {
                result = System.Text.Json.JsonSerializer.Deserialize<T>(text);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
