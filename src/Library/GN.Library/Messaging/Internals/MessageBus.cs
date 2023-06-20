using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using GN.Library.Messaging.Transports;
using GN.Library.Messaging.Pipeline;
using Microsoft.Extensions.Logging;
using GN.Library.Contracts_Deprecated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using GN.Library.Shared;
using System.Diagnostics;
using GN.Library.Natilus.Messaging;
using GN.Library.Messaging.Messages;
using System.Reflection;
using GN.Library.Messaging.Queues;
using GN.Library.Shared.Messaging.Messages;

namespace GN.Library.Messaging.Internals
{
    class MessageBus : BackgroundService, IMessageBusEx, IMessagingServices, IMessageBusConfiguration, IProcedureCall
    {
        internal static MessageBus Bus;
        private readonly IServiceProvider serviceProvider;
        private MessageBusSubscriptions subscriptions = new MessageBusSubscriptions();
        private List<IMessageTransport> transports;
        private MessageBusConfigurator configurator;
        private ConcurrentDictionary<string, Request> requests = new ConcurrentDictionary<string, Request>();
        protected PipelineQueue queue;
        private Task subscriptionTasks;
        private ILogger logger;
        private CancellationToken _cancellationToken = default;
        public CancellationToken CancellationToken => this._cancellationToken;

        private IMemoryCache chache;
        List<IPipelineStep> steps;

        internal MessageBusSubscriptions Subscriptions => this.subscriptions;
        internal ConcurrentDictionary<string, Request> Requests => this.requests;
        public MessageBusOptions Options => this.configurator?.Options;
        public string EndpointName => this.configurator.Options.GetEndpointName();
        public MessageBus(IServiceProvider serviceProvider, MessageBusConfigurator configurator)
        {
            this.serviceProvider = serviceProvider;

            this.chache = this.serviceProvider.GetServiceEx<IMemoryCache>();
            this.logger = this.serviceProvider.GetServiceEx<ILogger<MessageBus>>();
            this.configurator = configurator ?? serviceProvider.GetServiceEx<MessageBusConfigurator>();
            this.configurator.Bus = this;
            this.queue = this.configurator.Options.NumberOfQueues > 0 ? new PipelineQueue(this.configurator.Options.NumberOfQueues)
                : null;
            Bus = Bus ?? this;
            this.serviceProvider.GetServiceEx<IEnumerable<Action<IMessageBusConfigurator>>>()
                .ToList()
                .ForEach(x =>
                {
                    x?.Invoke(this.configurator);
                });
            this.configurator.SubscriptionBuilders.Add(b =>
            {
                b.UseTopic(typeof(QueryHandler));
                b.UseHandler(ctxx =>
                {
                    return Task.CompletedTask;
                });
                return b.Subscribe();
            });
            this.subscriptionTasks = Task.WhenAll(this.configurator
                .SubscriptionBuilders
                .Select(x => x(this.CreateSubscription())));
        }

        public IServiceProvider ServiceProvider => this.serviceProvider;
        internal List<IMessageTransport> GetTransports()
        {
            if (this.transports == null)
            {
                this.transports = this.serviceProvider.GetServiceEx<IEnumerable<IMessageTransport>>()
                    .ToList();
                this.transports.ToList().ForEach(x => x.Init(this, this.HandleReceive));
            }
            return this.transports;

        }
        public IMessagingSerializationService Serializer => SerializationService.Default;
        public async Task HandleReceive(IMessageTransport transport, object body, IDictionary<string, object> headers)
        {
            await Task.CompletedTask;
            IMessageContext context = null;
            try
            {
                if (body != null && typeof(ReadOnlyMemory<byte>).IsAssignableFrom(body.GetType()))
                {
                    var bytes = (ReadOnlyMemory<byte>)body;
                    var pack = this.GetSerializationService().DecodeMessagePack(bytes.ToArray());
                    context = new MessageContext<object>(LogicalMessage.Unpack(pack), null, this);
                    this.logger.LogTrace(
                        $"EventBus received message. Id:{context?.Message?.MessageId}, Transport:{transport.Name}");
                }
                if (body != null && body.GetType() == typeof(string))
                {
                    var bytes = (string)body;
                    var pack = this.GetSerializationService().DeserializMessagePack(bytes);
                    try
                    {
                        context = new MessageContext<object>(LogicalMessage.Unpack(pack), null, this);
                        if (context.Message.From() == this.EndpointName)
                        {
                            return;
                        }
                    }
                    catch (Exception err)
                    {
                        this.logger.LogWarning(
                            $"Failed to deserialize Packet. Subject: '{pack?.Subject}' TypeName:'{pack.TypeName}'");
                        throw;
                    }
                    this.logger.LogDebug(
                       $"EventBus received message. Id:{context?.Message?.MessageId}, Transport:{transport.Name}");
                }
                if (context != null)
                {
                    context.GetOrSetTransport(transport);
                    await this.DoPublish(context, default(CancellationToken));
                }
                else
                {
                    this.logger.LogWarning(
                        "Failed to deserialize message recived by transport.");
                }
            }
            catch (Exception err)
            {
                this.logger.LogWarning("An error occured while trying to handle received message. Err:{0}", err.GetBaseException());

            }
        }
        IMessagingServices IMessageBusConfiguration.ServiceProvider => this;

        //public IServiceProvider ServiceProvider => this.serviceProvider;

        public IMessageBusConfiguration Configuration => this;

        public IProcedureCall Rpc => this;

        public IMessageContext<T> CreateContext<T>(MessageTopic topic, T message)
        {
            if (message == null)
            {
                throw new ArgumentException(nameof(message));
            }
            return new MessageContext<T>(new LogicalMessage<T>(topic, message, null), null, this);
        }
        public IMessageContext<T> CreateMessage<T>(T message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message),$"Type:{typeof(T).FullName}");
            }
            //if (message is MessagePack pack)
            //{
            //    var packed = LogicalMessage.Unpack(pack);
            //}
            return new MessageContext<T>(new LogicalMessage<T>(MessageTopic.Create(typeof(T)), message, null), null, this);
        }

        //public Task<IMessageBusSubscription> Subscribe(Action<IMessageBusSubscription> configure)
        //{
        //    //await Task.CompletedTask;
        //    IMessageBusSubscription result = new MessageBusSubscription();
        //    configure?.Invoke(result);
        //    return this.Subscribe(result);
        //}

        //public void ReplayStream(IMessageBusSubscription subscription)
        //{
        //    if (!string.IsNullOrWhiteSpace(subscription.Topic.Stream))
        //    {
        //    }

        //}

        public async Task<IMessageBusSubscription> Subscribe(IMessageBusSubscription subscription)
        {
            this.subscriptions.Add(subscription);
            /// if there is an stream 
            /// 
            if (!string.IsNullOrWhiteSpace(subscription.Topic?.Stream))
            {
                var cmd = new OpenStream
                {
                    TopicFilter = subscription.Topic.Subject,
                    Stream = subscription.Topic.Stream,
                    Position = subscription.Topic.FromVersion,
                    Id = subscription.Properties.RemoteId(null) ?? subscription.Id.ToString()
                };
                await this.CreateMessage(cmd)
                    .UseTopic(MessagingConstants.Topics.OpenStream)
                    .Publish();
            }
            if (!string.IsNullOrWhiteSpace(subscription.QueueName))
            {
                await this.serviceProvider.GetServiceEx<IQueueManagerService>()
                    .Subscribe(subscription, this.CancellationToken);
               

            }
            foreach (var transport in this.GetTransports())
            {
                await transport.Subscribe(subscription);

            }
            return subscription;
        }
        
        private List<IPipelineStep> GetPipelineSteps(bool refersh = false)
        {
            if (this.steps == null || refersh)
            {
                this.steps = new List<IPipelineStep>();
                //this.steps.AddRange(this.Configuration.GetConfiguredSteps(Pipeline.Pipelines.Outgoing));
                this.steps.Add(new PrePublishStep());

                //this.steps.Add(new PipelineStep(InvalidMessageStep));
                this.steps.Add(new InvalidMessageStep(this));
                //this.steps.Add(new PipelineStep(ControlStep));
                this.steps.Add(new ControlStep(this));
                this.steps.Add(new SaveToStreamStep());
                this.steps.Add(new QueueStep());
                //this.steps.Add(new PipelineStep(HandleReplyStep));
                this.steps.Add(new HandleReplyStep(this));
                this.steps.AddRange(this.Configuration.GetConfiguredSteps(GN.Library.Messaging.Pipeline.Pipelines.Outgoing));
                //this.steps.Add(new PipelineStep(this.InternalPublishStep));
                this.steps.Add(new PublishToSubscribersStep(this));
                //this.steps.Add(new PipelineStep(this.TransportPublishStep));
                this.steps.Add(new PublishToTransoprtsStep(this));
            }
            return this.steps;
        }

        public PipelineContext GetPublishPipelineContext(IMessageContext message, CancellationToken cancellationToken)
        {
            //var options = message.GetPublishOptions();
            var steps = this.GetPipelineSteps().ToArray();
            //List<IPipelineStep> steps = new List<IPipelineStep>();
            //steps.Add(new PipelineStep(InvalidMessageStep));
            //steps.Add(new PipelineStep(ControlStep));
            //steps.Add(new SaveToStreamStep());
            //steps.Add(new PipelineStep(HandleReplyStep));
            //steps.AddRange(this.Configuration.GetSteps(GN.Library.Messaging.Pipeline.Pipelines.Outgoing));
            //steps.Add(new PipelineStep(this.InternalPublishStep));
            //var options = message.GetPublishOptions();
            //if (!options.LocalOnly)
            //{
            //    steps.Add(new PipelineStep(this.TransportPublishStep));
            //}
            return new PipelineContext(this, message, steps)
            {
                CancellationToken = cancellationToken
            };
        }

        public Task DoPublish(IMessageContext message, CancellationToken cancellationToken)
        {

            return this.queue != null && this.queue.Started
                ? this.queue.Enqueue(GetPublishPipelineContext(message, cancellationToken))
                : GetPublishPipelineContext(message, cancellationToken).Invoke();
            //return this.queue.Enqueue(GetPublishPipelineContext(message, cancellationToken));
        }


        public Task Publish(IMessageContext message, CancellationToken cancellationToken = default)
        {

            return DoPublish(message, cancellationToken);
        }
        //}
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            if (this.subscriptionTasks == null)
            {
                this.subscriptionTasks = Task.WhenAll(this.configurator
                    .SubscriptionBuilders
                    .Select(x => x(this.CreateSubscription())));
            }
            var handlers = this.serviceProvider.GetServiceEx<IEnumerable<IMessageHandlerConfigurator>>();
            var tasks = handlers.Select(x =>
            {
                var builder = new SubscriptionBuilder(this);
                x.Configure(builder);
                return builder.Subscribe();
            }).ToList();
            var sp = this.serviceProvider;
            foreach (var handler in sp.GetServiceEx<IEnumerable<IMessageHandler>>())
            {
                foreach (var _type in handler.GetType()
                    .GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IMessageHandler<>) && x.GenericTypeArguments.Length == 1)
                    .Select(x => x.GenericTypeArguments[0]))
                {
                    var method = handler.GetType().GetMethods()
                        .FirstOrDefault(x =>
                            x.GetParameters().Length == 1 &&
                            x.GetParameters()[0].ParameterType == typeof(IMessageContext<>).MakeGenericType(_type));
                    var topic = $"{handler.GetType().GetCustomAttribute<MessageHandlerAttribute>()?.Topic},{method.GetCustomAttribute<MessageHandlerAttribute>()?.Topic},{MessageTopicHelper.GetTopicByType(_type)}";
                    var builder = new SubscriptionBuilder(this)
                        .UseTopic(topic)
                        .UseHandler(_ctx =>
                        {
                            if (_ctx.TryCastEx(_type, out var __ctx))
                            {
                                return (Task)method.Invoke(handler, new object[] { __ctx });

                            }
                            else
                            {
                                throw new Exception($"Unexpected Cast Error! {_type.FullName}");
                            }
                        });
                    tasks.Add(builder.Subscribe());
                }
            }
            return Task.WhenAll(Task.WhenAll(this.subscriptionTasks), Task.WhenAll(tasks), base.StartAsync(cancellationToken));
        }
        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var tarns in this.GetTransports())
            {
                await tarns.StopAsync(cancellationToken);
            }
            await base.StopAsync(cancellationToken);
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this._cancellationToken = stoppingToken;
            return this.queue != null
                ? this.queue.Start(stoppingToken)
                : Task.CompletedTask;

            //return Task.Delay(10);

        }

        public ISubscriptionBuilder CreateSubscription<TM>(Func<IMessageContext<TM>, Task> handler = null)
        {
            return new SubscriptionBuilder(this).UseHandler(handler)
                .UseTopic(typeof(TM));
        }

        public ISubscriptionBuilder CreateSubscription(Func<IMessageContext, Task> handler = null)
        {
            return new SubscriptionBuilder(this).UseHandler(handler);
        }

        public object GetService(Type serviceType)
        {
            return this.serviceProvider.GetService(serviceType);
        }

        public IEnumerable<IPipelineStep> GetConfiguredSteps(GN.Library.Messaging.Pipeline.Pipelines pipeline)
        {
            return this.configurator.Steps.Where(x => x.Pipeline == pipeline).OrderBy(x => x.Rank)
                .Select(x => x.constructor(this));
        }

        public Task SaveToStream(IMessageContext[] messages)
        {
            throw new NotImplementedException();
        }


        public async Task SaveToStream(ILogicalMessage x, bool skipPublish = false)
        {

            if (x != null && !string.IsNullOrWhiteSpace(x.Stream))
            {
                if (skipPublish)
                {
                    var req = this.CreateMessage(new SaveEventToStream()
                    {
                        Events = new MessagePack[] { x.Pack() },
                        Stream = x.Stream,
                        SkipPublish = skipPublish
                    })
                    .UseTopic(MessagingConstants.Topics.SaveEvent)
                    .CreateRequest();
                    var reply = await req.WaitFor(y => true).TimeOutAfter(this.Options.DefaultTimeout, throwIfTimeOut: false);
                    if (reply != null && reply.Message.TryCast<SaveEventToStreamRespond>(out var m) && m.Body.Versions.Length > 0)
                    {
                        x?.WithVersion(m.Body.Versions[0]);

                    }
                }
                else
                {
                    await this.CreateMessage(new SaveEventToStream()
                    {
                        Events = new MessagePack[] { x.Pack() },
                        Stream = x.Stream,
                        SkipPublish = skipPublish
                    })
                    .UseTopic(MessagingConstants.Topics.SaveEvent)
                    .Publish();
                }
            }
        }

        public async Task SaveToStream(object[] events, string stream)
        {
            var packs = (events ?? new object[] { })
                .Where(x => x != null)
                .Select(x => new LogicalMessage(MessageTopic.Create(x.GetType(), stream), x, null))
                .ToArray()
                .Select(x => x.Pack())
                .ToArray();
            await this.CreateMessage(new SaveEventToStream()
            {
                Events = packs,
                Stream = stream,
            })
            .UseTopic(MessagingConstants.Topics.SaveEvent)
            .Publish();
        }
        public IMessageBusEx Advanced()
        {
            return this;
        }
        public IRequest CreateRequest(IMessageContext message, RequestOptions options = null)
        {
            message.IsRequest(true);
            var result = this.requests.GetOrAdd(message.Message.MessageId, new Request((t) =>
            {
                return this.DoPublish(message, t);
            }, message, this, options));
            return result;

        }
        public Task<TResponse> Call<TRequest, TResponse>(TRequest request, string subject, int timeOut = LibraryConstants.DefaultTimeout)
        {
            return this.Call<TRequest, TResponse>(request, timeOut, subject);
        }
        public async Task<TResponse> Call<TRequest, TResponse>(TRequest request, int timeOut = LibraryConstants.DefaultTimeout, string subject = null)
        {
            var _request = subject == null
                ? this.CreateMessage(request)
                    .UseTopic(typeof(TRequest))
                    .CreateRequest()
                : this.CreateMessage(request)
                     .UseTopic(subject)
                     .CreateRequest();

            var reply = await _request.WaitFor(x => true).TimeOutAfter(timeOut);
            if (reply != null && reply.Message != null && reply.Message.TryCast<Exception>(out var _exp))
            {
                throw _exp.Body;
            }
            return reply != null && reply.Message != null && reply.Message.TryCast<TResponse>(out var _res) ? _res.Body : default(TResponse);
        }

        public bool IsConnected(int timeout)
        {
            var result = true;
            foreach (var transport in this.GetTransports())
            {
                result = result && transport.IsConnected(timeout);
            }
            return result;
        }

        public virtual INatilusMessageContext CreateNatilusMessage(string subject, object message)
        {
            throw new NotImplementedException();
        }

        public virtual INatilusSubscriptionBuilder CreateNatilusSubscription(string subject)
        {
            throw new NotImplementedException();
        }

        public void CancelRequest(IMessageContext request)
        {
            this.requests.TryRemove(request.Message.MessageId, out var _);
        }

        public IMessageContext CreateMessageContext(MessagePack message)
        {
            var result = new MessageContext<object>(LogicalMessage.Unpack(message), null, this);
            return result;

        }

        public Task Enqueue(IMessageContext context)
        {
            var manager = this.ServiceProvider.GetService<IQueueManagerService>();
            return manager.Enqueue(context,this._cancellationToken);

            
        }
    }
}
