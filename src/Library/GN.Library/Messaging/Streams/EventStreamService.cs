using GN.Library.Messaging.Internals;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using GN.Library.Messaging.Messages;

namespace GN.Library.Messaging.Streams
{
    class StreamReplayer
    {
        public Guid Id { get; set; }
        public string Stream { get; set; }
        public string StreamId { get; set; }
        public long StartPos { get; set; }
        public long Position { get; set; }

    }
    class EventStreamService : BackgroundService
    {
        private IMessageBusEx bus;
        private IStreamManager streamManager;
        private IMessagingServices services;
        private IMessagingSerializationService serializer;
        private ConcurrentDictionary<Guid, StreamReplayer> openStreams = new ConcurrentDictionary<Guid, StreamReplayer>();
        private ConcurrentDictionary<string, StreamConsumer> consumers = new ConcurrentDictionary<string, StreamConsumer>();
        private CancellationTokenSource consumerToken = new CancellationTokenSource();
        private ILogger<EventStreamService> logger;

        public EventStreamService(IMessagingServices services, ILogger<EventStreamService> logger)
        {
            this.bus = services.GetEventBusEx();
            this.streamManager = services.GetServiceEx<IStreamManager>();
            this.serializer = services.GetSerializationService();
            this.logger = this.bus.Advanced().ServiceProvider.GetServiceEx<ILogger<EventStreamService>>();
        }

        private async Task Publish(MessagePack x, string stream, Action<IMessageContext> configure)
        {
            var message = this.bus.CreateMessage(x)
                        .UseTopic(x.Subject, stream, x.Version);// MessageSubject.Create(x.Name, stream,  x.Version));
            //.ExtendHeaders(additionalHeaders);
            configure?.Invoke(message);
            //message.Message.SkipSaveToStream(true);
            //message.Message.ReplayFor(replayFor);
            await message.Publish();
        }
        private async Task HandleReplayCommand(IMessageContext message)
        {
            var request = message.Cast<ReplayStreamCommand>()?.Message?.Body;
            if (request != null && request.Position.HasValue)
            {
                var stream = await this.streamManager.GetStream(request.Stream);
                if (stream != null)
                {
                    await stream?.ReplayEx(async ctx =>
                    {
                        await message.Reply(new ReplayStreamReply
                        {
                            Events = ctx.Events,
                            Position = ctx.Position,
                            Remaining = ctx.Remaining

                        });

                    }, request.Position, request.ChunkSize);
                }
            }
        }
        private StreamConsumer GetOrAdd(IMessageContext<OpenStream> message, IStream stream)
        {
            var consumerName = $"{message.Message.From()}-{message.Message.Body.Id}-{message.Message.Body.TopicFilter}";
            return this.consumers.GetOrAdd(consumerName, (x) =>
            {
                return new StreamConsumer(message, this.bus, stream);
            });

        }
        private StreamConsumer[] GetConsumer(IMessageContext messag)
        {
            return new StreamConsumer[] { };
        }
        private async Task HandleOpen_deprecated(IMessageContext message)
        {
            var request = message.Cast<OpenStream>()?.Message?.Body;
            this.logger.LogInformation(
                $"Openning Stream: {request.Stream}");
            if (request != null)
            {
                var stream = await this.streamManager.GetStream(request.Stream);
                var strategy = 0;
                switch (strategy)
                {
                    case 1:
                        await stream.Replay(async x =>
                        {
                            await this.Publish(x, request.Stream, m =>
                            {
                                m.Message.ReplayFor(request.Id.ToString());
                            });
                            return true;
                        }, request.Position);
                        break;
                    case 2:
                        if (stream != null)
                        {

                            await stream?.ReplayEx(async ctx =>
                            {
                                int sent_count = 0;

                                foreach (var ev in ctx.Events)
                                {
                                    await this.Publish(ev, request.Stream, m =>
                                    {
                                        m.Message.ReplayFor(request.Id.ToString());
                                        //message.Message.From();
                                        //m.Message.Headers.TrySetValue("$replay-remaining", (ctx.Remaining + ctx.Events.Length) - sent_count);
                                        m.Message.Headers.ReplayRemainingCount((ctx.Remaining + ctx.Events.Length) - sent_count);

                                        //m.Message.Headers.Extend(clone.Message.Headers);
                                    });// reuest.Id.ToString());
                                    sent_count++;
                                }

                            }, request.Position, request.ChunkSize);
                        }
                        break;
                    default:
                        var consumer = this.GetOrAdd(message.Cast<OpenStream>(), stream);
                        _ = consumer.Run(this.consumerToken.Token);
                        break;

                }

            }
            await Task.CompletedTask;
        }
        private async Task HandleOpen(IMessageContext message)
        {
            var request = message.Cast<OpenStream>()?.Message?.Body;
            this.logger.LogInformation(
                $"Openning Stream: {request.Stream}");
            if (request != null)
            {
                var stream = await this.streamManager.GetStream(request.Stream);
                var consumer = this.GetOrAdd(message.Cast<OpenStream>(), stream);
                _ = consumer.Run(this.consumerToken.Token);
            }
            await Task.CompletedTask;
        }
        private async Task HandleSave(IMessageContext message)
        {
            var reply = new SaveEventToStreamRespond();
            var save_event = message.Cast<SaveEventToStream>()?.Message?.Body;
            if (save_event != null)
            {
                List<MessagePack> _events = new List<MessagePack>();
                _events = (save_event.Events ?? new object[] { })
                    .Select(x => this.serializer.ToVersionableEvent(x))
                    .ToList();
                if (!AppHost.Conventions.IsStreamNameValid(save_event.Stream))
                {
                    throw new Exception($"Invalid stream name:{save_event.Stream}");
                }
                if (_events.Count > 0)
                {
                    using (var stream = await this.streamManager.GetStream(save_event.Stream, true))
                    {
                        if (stream != null)
                        {
                            var new_events = await stream.SaveAsync(_events);
                            reply.Versions = new_events.Select(x => x.GetVersion()).ToArray();
                            if (!save_event.SkipPublish)
                            {
                                foreach (var x in new_events)
                                {
                                    var topic = MessageTopic.Create(x.Subject, save_event.Stream, x.Version);
                                    var consumers = this.consumers.Values.Where(c => c.Matches(topic)).ToArray();
                                    foreach (var c in consumers)
                                    {
                                        await c.Publish(x, save_event.Stream, null);
                                    }
                                }
                            }
                            foreach (var x in new_events)
                            {
                                //if (!save_event.SkipPublish)
                                //    await this.Publish(x, save_event.Stream, save_event.StreamId, null);

                            }
                        }
                    }
                }
            }
            await message.Reply(reply);
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation(
                $"Streaming Services Started");
            await this.bus.CreateSubscription(x => this.HandleSave(x))
                .UseTopic(MessagingConstants.Topics.SaveEvent) //, "*", "*")
                .Subscribe();
            await this.bus.CreateSubscription(x => this.HandleOpen(x))
                .UseTopic(MessagingConstants.Topics.OpenStream)
                .Subscribe();
            await this.bus.CreateSubscription(x => this.HandleReplayCommand(x))
                .UseTopic(MessagingConstants.Topics.ReplayStream)
                .Subscribe();


            await base.StartAsync(cancellationToken);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            this.consumerToken.Cancel();
            return base.StopAsync(cancellationToken);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Delay(100);
        }
    }
}
