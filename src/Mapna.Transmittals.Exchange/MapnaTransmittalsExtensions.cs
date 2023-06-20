using Mapna.Transmittals.Exchange.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Mapna.Transmittals.Exchange.Services;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Hosting;
using Mapna.Transmittals.Exchange.Services.Queues.Incomming;
using Mapna.Transmittals.Exchange.Domain.Outgoing;
using System.Text;

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

            services.AddSingleton<TransmittalOutgoingQueue>();
            services.AddSingleton<IOutgoingQueue>(sp => sp.GetService<TransmittalOutgoingQueue>());
            services.AddSingleton<IHostedService>(sp => sp.GetService<TransmittalOutgoingQueue>());



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
        internal static bool TryDeserialize<T>(string text, out T result, bool useNewtonSoft = false)
        {
            result = default(T);
            try
            {
                if (useNewtonSoft)
                {
                    result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(text);
                }
                else
                {
                    result = System.Text.Json.JsonSerializer.Deserialize<T>(text);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        

        internal static string ToUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;
            return $"http://77.104.99.86:9000/Transmittals/File/{System.Convert.ToBase64String(Encoding.UTF8.GetBytes(path))}";
        }

        internal static string GetDownloadableUrl(this Microsoft.SharePoint.Client.File file)
        {
            if (file == null)
                return null;
            return ToUrl(file?.ServerRelativeUrl);
            //return $"http://172.16.6.78:2450/Transmittal/File/{System.Convert.ToBase64String(Encoding.UTF8.GetBytes(file.ServerRelativeUrl))}";
        }
    }
}
