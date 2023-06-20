
using GN.Library.Messaging;
using GN.Library.Messaging.Queues;
using GN.Library.ServiceDiscovery;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MessagingQueueServiceCollectionExtensions
    {
        public static IServiceCollection AddMessagingQueue(this IServiceCollection services, IConfiguration configuration, Action<MessagingQueueOptions> configure)
        {
            var options = configuration.GetMessaginQueueOptions();
            configure?.Invoke(options.Validate());
            return AddMessagingQueue(services,options);
        }
        public static IServiceCollection AddMessagingQueue(this IServiceCollection services, MessagingQueueOptions options)
        {
            options = options.Validate();
            services.AddSingleton(options);
            if (options.Enabled)
            {
                services.AddSingleton<LocalQueueService>();
                services.AddTransient<ILocalQueueService>(sp => sp.GetService<LocalQueueService>());
                services.AddTransient<IMessageHandler, CreateQueueCommandHandler>();
                services.AddTransient<IMessageHandler, GetQueuesInformationHandler>();
                services.AddTransient<IMessageHandler, EnqueueHandler>();
                services.AddTransient<IMessageHandler, QueueSubscribeHandler>();
                services.AddTransient<IServiceDataProvider>(sp => sp.GetService<LocalQueueService>());
            }
            return services;
        }
        public static MessagingQueueOptions GetMessaginQueueOptions(this IConfiguration configuration)
        {
            return configuration
                .GetSection("messaging")?
                .GetSection("queues")?
                .Get<MessagingQueueOptions>() ?? new MessagingQueueOptions();
        }
    }
}
