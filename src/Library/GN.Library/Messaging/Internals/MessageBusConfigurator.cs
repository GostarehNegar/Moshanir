using GN.Library.Shared;
using GN.Library.Shared;
using GN.Library.Messaging.Pipeline;
using GN.Library.Messaging.Streams;
using GN.Library.Messaging.Streams.LiteDb;
using GN.Library.Natilus.Messaging.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using GN.Library.Natilus;
using GN.Library.ServiceDiscovery;
using GN.Library.Messaging.Queues;

namespace GN.Library.Messaging.Internals
{
    class MessageBusConfigurator : IMessageBusConfigurator
    {
        internal class PipelineStepModel
        {
            public Func<IMessageBusConfiguration, IPipelineStep> constructor;
            public int Rank;
            public GN.Library.Messaging.Pipeline.Pipelines Pipeline;
        }
        internal static MessageBusConfigurator Instance = null;
        private ConcurrentDictionary<string, object> _properties = new ConcurrentDictionary<string, object>();
        internal List<PipelineStepModel> Steps = new List<PipelineStepModel>();
        public List<Func<ISubscriptionBuilder, Task>> SubscriptionBuilders =
            new List<Func<ISubscriptionBuilder, Task>>();
        public IServiceCollection ServiceCollection { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }
        public IMessageBus Bus { get; internal set; }

        public IDictionary<string, object> Properties => this._properties;

        public MessageBusOptions Options { get; private set; }
        public IConfiguration Configurations { get; private set; }

        public MessageBusConfigurator(IServiceCollection services, IConfiguration configurations, Action<MessageBusOptions> configure = null)
        {
            this.ServiceCollection = services;
            this.Configurations = configurations;
            this.Options = new MessageBusOptions().Validate();
            configurations.GetSection("messaging").Bind("bus", this.Options);
            configure?.Invoke(this.Options);
            services.AddSingleton(this.Options);

            if (1 == 1) // if !Natilus Bus
            {
                services.AddSingleton<MessageBus>(s => new MessageBus(s, this));
            }
            else
            {
                ///
                /// Deprecated Nastilus Bus
                /// 
                //services.AddSingleton<MessageBus>(s => new NatilusBus(s));
                //services.AddNatilus(configurations, c => { });
            }
            services.AddSingleton<MessageBusConfigurator>(this);
            services.AddSingleton<IProcedureCall>(s => s.GetServiceEx<MessageBus>());
            services.AddTransient<IMessageBusEx>(s => s.GetServiceEx<MessageBus>());
            services.AddTransient<IMessageBus>(s => s.GetServiceEx<MessageBus>());
            services.AddTransient<IHostedService>(s => s.GetServiceEx<MessageBus>());
            services.AddTransient<IMessagingServices>(s => s.GetServiceEx<MessageBus>());
            services.AddTransient<IMessageBusConfiguration>(s => s.GetServiceEx<MessageBus>());

            services.AddSingleton<NodeService>();
            services.AddSingleton<IServiceDiscovery>(sp => sp.GetServiceEx<NodeService>());
            services.AddHostedService(sp => sp.GetServiceEx<NodeService>());


            services.AddSingleton<SerializationService>();
            services.AddSingleton<IMessagingSerializationService>(s => s.GetServiceEx<SerializationService>());
            services.AddSingleton<QueueManagerService>();
            services.AddTransient<IQueueManagerService>(sp => sp.GetService<QueueManagerService>());
            if (this.Options.AddStreamingServices)
            {
                services.AddSingleton<LiteDbStreamManager>();
                services.AddSingleton<IStreamManager>(s => s.GetServiceEx<LiteDbStreamManager>());
                services.AddSingleton<EventStreamService>();
                services.AddSingleton<IHostedService>(s => s.GetServiceEx<EventStreamService>());
            }

            /// Add default steps
            /// 
            this.AddPipelineStep(s => new PrePublishStep(), GN.Library.Messaging.Pipeline.Pipelines.Outgoing, 0);
        }

        public IMessageBusConfigurator Register(Action<ISubscriptionBuilder> configure)
        {
            this.SubscriptionBuilders.Add(s =>
            {
                configure?.Invoke(s);
                return s.Subscribe();
            });
            return this;
        }

        public void AddPipelineStep(Func<IMessageBusConfiguration, IPipelineStep> constructor, GN.Library.Messaging.Pipeline.Pipelines pipeline, int rank = 1000)
        {

            this.Steps.Add(new PipelineStepModel { constructor = constructor, Rank = rank });
        }
    }
}
