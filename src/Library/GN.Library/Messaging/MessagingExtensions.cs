
using GN.Library.Messaging.Internals;
using GN.Library.Messaging.Streams;
using GN.Library.Messaging.Transports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using GN.Library.Shared.Entities;
using GN.Library.Messaging.Messages;

namespace GN.Library.Messaging
{
    public static partial class MessagingExtensions
    {

        /// <summary>
        /// Adds a message handler for messages of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static IServiceCollection AddMessageHandler<T>(this IServiceCollection services, Func<IMessageContext<T>, Task> handler)
        {

            return AddMessagingServices(services, cfg => cfg.Register(_ =>
            {
                _.UseTopic(typeof(T))
                .UseHandler(handler);
            }));
        }
        /// <summary>
        /// Adds a message handler for messages of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static IServiceCollection AddMessageHandler(this IServiceCollection services, Type type, Func<IMessageContext, Task> handler)
        {
            return AddMessagingServices(services, cfg => cfg.Register(_ =>
            {
                _.UseTopic(type)
                .UseHandler(handler);
            }));
        }
        /// <summary>
        /// Adds a message handler for messsage with the specified topic;
        /// </summary>
        /// <param name="services"></param>
        /// <param name="topic">The topic to  subscribe to. Normally topic are the FullName of the message
        /// type e.g 'Portal.Sale.QuoteCreated' . Topic may include wildcards such as 'Portal.Sale.*'. </param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static IServiceCollection AddMessageHandler(this IServiceCollection services, string topic, Func<IMessageContext, Task> handler)
        {
            return AddMessagingServices(services, cfg => cfg.Register(_ =>
            {
                _.UseTopic(topic)
                .UseHandler(handler);
            }));
        }
        /// <summary>
        /// Adds a message handler using the subscription builder interface.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="builder"> Delgate to configure the subscription.</param>
        /// <returns></returns>
        public static IServiceCollection AddMessageHandler(this IServiceCollection services, Action<ISubscriptionBuilder> builder)
        {
            return AddMessagingServices(services, cfg => cfg.Register(builder));
        }
        /// <summary>
        /// Adds a configurator delegate.
        /// This is specially used in libraries where a full configuration of the
        /// bus is not intended. For example to register a handler. 
        /// The bus will eventually be configured thru the service container initialization, when
        /// these delegates will be invoked.
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configurator"></param>
        /// <returns></returns>

        public static IServiceCollection AddMessageHandler<T>(this IServiceCollection services, bool singleton = true) where T : class, IMessageHandler
        {
            if (singleton)
                services.AddSingleton<IMessageHandler, T>();
            else
                services.AddTransient<IMessageHandler, T>();
            return services;
        }

        public static IServiceCollection AddMessagingServices(this IServiceCollection services, Action<IMessageBusConfigurator> configurator)
        {
            services.AddSingleton<Action<IMessageBusConfigurator>>(configurator);
            return services;
        }
        public static IMessageBusConfigurator AddMessagingServices(this IServiceCollection services, IConfiguration configurations, Action<MessageBusOptions> configure = null)
        {
            if (MessageBusConfigurator.Instance == null)
            {
                MessageBusConfigurator.Instance = new MessageBusConfigurator(services, configurations, configure);
            }
            return MessageBusConfigurator.Instance;
        }
        public static IMessagingServices Services => MessageBus.Bus;
        public static IMessageBus GetEventBus(this IMessagingServices services)
        {
            return services.GetEventBusEx();
        }
        internal static IMessageBusEx GetEventBusEx(this IMessagingServices services)
        {
            return services.GetServiceEx<IMessageBusEx>();
        }
        internal static SerializationService GetSerializationService(this IMessagingServices serviceProvider)
        {
            return SerializationService.Default as SerializationService;
        }


        internal static ILock Lock(this IMessageContext context, bool isDisposable = false)
        {
            return context.ServiceProvider.GetServiceEx<ILockManager>().Lock(context.Message.MessageId.ToString(), isDisposable);
        }
        public static bool IsRequest(this IMessageContext context, bool? value = null) => context.Message.IsRequest(value);
        public static bool IsRequest(this ILogicalMessage context, bool? value = null) => IsRequest(context.Headers, value);
        public static bool IsRequest(this IMessageHeader header, bool? value = null)
        {
            if (value.HasValue)
            {
                var flags = header.Flags();
                if (value.Value)
                    flags |= MessageFlags.Request;
                else
                    flags &= ~MessageFlags.Request;
                header.Flags(flags);
            }
            return (header.Flags() & MessageFlags.Request) == MessageFlags.Request;
        }


        internal static bool HasBeenPublishedBy(this IMessageContext context, string endpointName)
        {
            var message = context?.Message;
            var result = false;
            if (message != null)
            {
                var visited = message.Headers.GetValue<string>(MessagingConstants.HeaderKeys.VisitedEndpoints)
                    ?? "";
                var endpointTag = $"+{endpointName}+";
                result = visited.Contains(endpointTag);
                if (!result)
                {
                    message.Headers.TrySetValue<string>(MessagingConstants.HeaderKeys.VisitedEndpoints,
                        visited + endpointTag);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true if the message is a reply.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool IsReply(this IMessageContext message)
        {
            var result = message != null && message.Message != null && message.Headers.HasFlag(MessageFlags.Reply) && !string.IsNullOrEmpty(message.Headers.InReplyTo()); /// message.Message.Subject == MessagingConstants.Topics.Reply;
            //var result = message != null && message.Message != null &&  message.Message.Subject == MessagingConstants.Topics.Reply;

            return result;
        }
        public static bool IsAquire(this IMessageContext message)
        {
            var result = message != null && message.Message != null && message.Message.Subject == MessagingConstants.Topics.Acquire;

            return result;
        }
        public static IMessageContext<T> CreateMessage<T>(this IMessagingServices service, T message)
        {
            return service.GetEventBusEx().CreateMessage<T>(message);
        }

        public static Task AsTask(this CancellationToken cancellationToken)
        {
            var source = new TaskCompletionSource<object>();
            cancellationToken.Register(() => source.TrySetCanceled(), false);
            return source.Task;
        }


        public static IMessageContext LocalOnly(this IMessageContext context, bool value)
        {
            context.Options().LocalOnly = value;
            return context;
        }
        public static MessageOptions Options(this IMessageContext context)
        {
            return context
                .Properties
                .GetValue<MessageOptions>("$publish_options", () => MessageOptions.GetDefault());

        }
        public static IMessageTransport GetOrSetTransport(this IMessageContext message, IMessageTransport transport = null)
        {
            return transport != null
                ? message.Properties.AddOrUpdateObjectValue(() => transport, "#transport")
                : message.Properties.GetObjectValue<IMessageTransport>("#transport");



        }
        public static object InternalEvent(this MessagePack ev)
        {
            var type = MessageTopicHelper.GetTypeByName(ev.Subject) ?? typeof(DynamicEntity);
            return Newtonsoft.Json.JsonConvert.DeserializeObject(ev.Payload, type);

            return null;
        }
        public static void AddProjection(this IMessageBusEx bus, string stream)
        {

        }
        public static long? Version(this LogicalMessage message)
        {
            return message?.Version;
        }
        public static long? Version(this IMessageContext message)
        {
            return message?.Message?.Version;
        }


        public static bool IsReplayedForMe(this IMessageBusSubscription subscription, IMessageContext message)
        {
            return subscription != null && message != null && message?.Message.ReplayFor() == subscription.Id.ToString();
        }

        public static void Extend(this IDictionary<string, string> headers, IDictionary<string, string> additionalHeaders)
        {
            if (headers != null && additionalHeaders != null)
            {
                foreach (var value in additionalHeaders)
                {
                    if (!headers.ContainsKey(value.Key))
                    {
                        headers.Add(value.Key, value.Value);
                    }
                }
            }

        }
        public static IMessageContext ExtendHeaders(this IMessageContext message, IDictionary<string, string> additionalHeaders)
        {
            message.Message.Headers.Extend(additionalHeaders);
            return message;
        }
        public static IMessageContext ExtendHeaders(this IMessageContext message, string key, string value)
        {

            return message;
        }

        public static long? ReplayRemainingCount(this IMessageHeader header, long? value = null)
        {
            if (header == null)
                return null;
            if (value.HasValue)
            {
                header.TrySetValue("$replay-remain_count", value);
            }
            return header.GetValue<long?>("$replay-remain_count");

        }

        public static IMessageContext Options(this IMessageContext message, Action<MessageOptions> cfg)
        {
            cfg?.Invoke(message.Options());
            return message;
        }
        private static async Task _SaveToStreamEx(IMessageBusEx bus, object[] events, string stream, string streamId, bool skipPublish = false)
        {
            var packs = (events ?? new object[] { })
                .Where(x => x != null)
                .Select(x => new LogicalMessage(
                        MessageTopic.Create(x.GetType(), stream), x, null))
                .ToArray()
                .Select(x => new MessagePack(-1, x.Pack().Payload, x.Pack().GetTopic().Subject, DateTime.UtcNow))
                .ToArray();
            await bus.CreateMessage(new SaveEventToStream()
            {
                Events = packs,
                Stream = stream,
                //StreamId = streamId,
                SkipPublish = skipPublish

            })
            .UseTopic(MessagingConstants.Topics.SaveEvent)
            //.Publish();
            .CreateRequest()
            .WaitFor(ctx =>
            {
                return true;
            });

            //.UseRouting("$new"), stream, streamId)

        }

        private static object[] getpage(object[] source, int page, int pageLenght)
        {
            return source.Skip(page * pageLenght).Take(pageLenght).ToArray();
        }
        public static async Task SaveToStreamEx(this IMessageBusEx bus, object[] events, string stream, string streamId, bool skipPublish = false)
        {
            var pageLength = 200;
            var page = 0;
            var fin = false;
            while (!fin)
            {
                var items = events
                    .Skip(page * pageLength)
                    .Take(pageLength)
                    .ToArray();
                page++;
                if (items.Length > 0)
                    await _SaveToStreamEx(bus, items, stream, streamId, skipPublish);
                else
                    break;

            }
            return;
            await _SaveToStreamEx(bus, events, stream, streamId, skipPublish);
            var packs = (events ?? new object[] { })
                .Where(x => x != null)
                .Select(x => new LogicalMessage(
                        MessageTopic.Create(x.GetType(), stream), x, null))
                .ToArray()
                .Select(x => new MessagePack(-1, x.Pack().Payload, x.Pack().GetTopic().Subject, DateTime.UtcNow))
                .ToArray();
            await bus.CreateMessage(new SaveEventToStream()
            {
                Events = packs,
                Stream = stream,
                //StreamId = streamId,
                SkipPublish = skipPublish

            })
            .UseTopic(MessagingConstants.Topics.SaveEvent)
            //.Publish();
            .CreateRequest()
            .WaitFor(ctx =>
            {
                return true;
            });

            //.UseRouting("$new"), stream, streamId)

        }

        public static async Task ReplayStream(this IMessageBusEx bus, Action<StreamReplayContext> callback, string stream,
            long Position = 0, int chunkSize = 1000, CancellationToken cancellationToken = default)
        {
            var request = bus.CreateMessage(new ReplayStreamCommand
            {
                Stream = stream,
                //StreamId = streamId,
                ChunkSize = chunkSize,
                Position = Position
            })
                .UseTopic(MessagingConstants.Topics.ReplayStream)
                .CreateRequest();

            //.Send();
            await request.WaitFor(ctx =>
            {
                var reply = ctx.Cast<ReplayStreamReply>();
                if (reply != null)
                {
                    var replayContext = new StreamReplayContext
                    {
                        Events = reply.Message.Body.Events
                                .Select(x => x as MessagePack)
                                .ToArray(),
                        RemaininCount = reply.Message.Body.Remaining,
                        Position = reply.Message.Body.Position,
                    };
                    callback?.Invoke(replayContext);
                    return reply.Message.Body.Remaining < 1 || replayContext.Stop;
                }
                return false;
            }, cancellationToken);
        }




    }
}
