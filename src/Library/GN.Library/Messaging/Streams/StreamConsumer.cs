using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using GN.Library.Messaging.Internals;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using GN.Library.Messaging.Messages;

namespace GN.Library.Messaging.Streams
{
    public class StreamConsumerConfig
    {
        //public string Stream { get; set; }
        //public string StreamId { get; set; }
        public string Stream => Topic?.Stream;
        public string Endpoint { get; set; }
        public string ConsumerSubscriptionId { get; set; }
        public int ChunkSize { get; set; }

        public string Key { get; set; }
        internal SubscriptionTopic Topic { get; private set; }

        internal static StreamConsumerConfig FromOpenStream(ILogicalMessage<OpenStream> o)
        {
            return new StreamConsumerConfig
            {
                Endpoint = o.From(),
                Topic = SubscriptionTopic.Create(o.Body.TopicFilter, o.Body.Stream, o.Body.Position),
                ChunkSize = o.Body.ChunkSize ?? 100,
                ConsumerSubscriptionId = o.Body.Id.ToString(),

            };
        }
        private string EndpointFriendlyName
        {
            get
            {
                if (Endpoint == null)
                    return null;
                return Endpoint.Split('x')[0];
            }
        }
        public override string ToString()
        {
            return $"Consumner: '{ConsumerSubscriptionId}@{EndpointFriendlyName}', Stream: '{Stream}' Pos:{Topic.FromVersion} ";
        }

    }
    class StreamConsumer
    {
        class VersionableTask
        {
            public string Stream { get; private set; }
            public string StreamId { get; private set; }
            public TaskCompletionSource<bool> Task { get; private set; }
            public MessagePack Event { get; set; }
            public VersionableTask(MessagePack ev, string streamName, string streamId)
            {
                this.Event = ev;
                this.Task = new TaskCompletionSource<bool>();
                this.Stream = streamName;
                this.StreamId = streamId;
            }
        }
        private readonly IMessageContext<OpenStream> request;
        private readonly IMessageBus bus;
        private readonly IStream stream;
        private BlockingCollection<VersionableTask> _items = new BlockingCollection<VersionableTask>();

        private DateTime? lastSeen;
        private bool consumer_disconnected;

        private object _lock = new object();
        private long lastVersion = -1;

        public StreamConsumerConfig Config { get; }
        private ILogger<StreamConsumer> logger;


        public StreamConsumer(IMessageContext<OpenStream> request, IMessageBus bus, IStream stream)
        {
            this.request = request;
            this.lastSeen = DateTime.UtcNow;
            this.Config = StreamConsumerConfig.FromOpenStream(request.Message);
            this.bus = bus;
            this.stream = stream;
            this.logger = this.bus.Advanced().ServiceProvider.GetServiceEx<ILogger<StreamConsumer>>();
        }

        private async Task DoPublishEx(PublishStreamData x, string stream, Action<IMessageContext> configure, TaskCompletionSource<bool> task = null)
        {
            x.Events.ToList().ForEach(_x => _x.Stream = stream);
            var message = this.bus.CreateMessage(x)
                .UseTopic("$stream-data");
            //.UseTopic(x.Name, stream, x.Version);
            configure?.Invoke(message);
            await message.Publish();
            //await message.CreateRequest().WaitFor(x1 => true).ConfigureAwait(false);
            await Task.Delay(1);
            //this.lastVersion = x.Version;
            task?.SetResult(true);
        }
        private async Task DoPublish(MessagePack x, string stream, Action<IMessageContext> configure, TaskCompletionSource<bool> task = null)
        {
            x.Stream = stream;
            var message = this.bus.CreateMessage(x)
                        .UseTopic(x.Subject, stream, x.Version);
            configure?.Invoke(message);
            await message.Publish();
            this.lastVersion = x.GetVersion();
            task?.SetResult(true);
        }
        public static bool WildCardMatch(string value, string pattern)
        {
            if (value == null && pattern == null)
                return true;
            if (value == null || pattern == null)
                return false;
            var exp = "^" + Regex.Escape(pattern).Replace("\\?", ".").Replace("\\*", ".*") + "$";
            return Regex.IsMatch(value, exp);
        }

        public bool Matches(MessageTopic topic)
        {
            var req = this.request.Message.Body;
            return (req.TopicFilter == topic.Subject || WildCardMatch(topic.Subject, req.TopicFilter))
                && (req.Stream == topic.Stream || WildCardMatch(topic.Stream, req.Stream))
                && (req.Position == null || topic.Version >= req.Position);
        }
        public bool Matches(IMessageContext ctx)
        {
            return this.Config.Topic.Matches(ctx.Message);
        }
        public Task<bool> Publish(MessagePack ev, string stream, string streamId)
        {
            var t = new VersionableTask(ev, stream, streamId);
            this._items.Add(t);
            return t.Task.Task;
        }
        public Task Run(CancellationToken token)
        {
            //    Task.Run(async () =>
            //    {
            //        while (!token.IsCancellationRequested)
            //        {
            //            await Task.Delay(5000);
            //            try
            //            {
            //                var reply = await this.bus.Rpc.Call<PingBus, PingBusReply>(new PingBus(), 5000);
            //                this.lastSeen = DateTime.UtcNow;
            //            }
            //            catch (Exception err)
            //            {
            //                this.lastSeen = null;
            //            }
            //        }
            //    });
            return Task.Run(async () =>
            {
                var req = this.request.Message.Body;
                var streamName = this.Config.Topic.Stream;
                var streamId = this.Config.Topic.StreamId;
                var chunkSize = this.Config.ChunkSize;
                var pos = this.Config.Topic.FromVersion;
                this.logger.LogInformation(
                    $"Consumer Starts: {this.Config}");
                this.lastVersion = -1;
                if (stream != null && pos.HasValue)
                {
                    await stream?.ReplayEx(async ctx =>
                    {
                        if (1 == 0)
                        {
                            int sent_count = 0;
                            foreach (var ev in ctx.Events)
                            {
                                this.lastVersion = ev.GetVersion();
                                await this.DoPublish(ev, streamName, m =>
                               {
                                   m.Message.ReplayFor(this.Config.ConsumerSubscriptionId);
                                   m.Message.Headers.ReplayRemainingCount((ctx.Remaining + ctx.Events.Length) - sent_count);
                                   m.Message.To(this.Config.Endpoint);
                                   //m.Message.Headers.IsVersiableEvent(true);
                               });
                                sent_count++;
                            }
                            this.logger.LogInformation(
                                $"Stream {sent_count} items sent.");
                        }
                        else
                        {
                            var newVersion = ctx.Events.LastOrDefault()?.Version;
                            if (newVersion.HasValue && newVersion > this.lastVersion)
                            {
                                this.lastVersion = newVersion.Value;
                            }
                            await this.DoPublishEx(new PublishStreamData { Stream = streamName, Events = ctx.Events }, streamName, m =>
                                {
                                    m.Message.ReplayFor(this.Config.ConsumerSubscriptionId);
                                    m.Message.Headers.ReplayRemainingCount((ctx.Remaining));
                                    m.Message.To(this.Config.Endpoint);
                                    //m.Message.Headers.IsVersiableEvent(true);
                                });
                        }
                    }, pos, chunkSize);
                }
                while (!token.IsCancellationRequested)
                {
                    if (1 == 1)
                    {
                        if (this._items.TryTake(out var item, 3000, token))
                        {
                            if (1 == 1 || item.Event.Version > this.lastVersion)
                            {
                                this.lastVersion = item.Event.GetVersion();
                                var data = new PublishStreamData
                                {
                                    Stream = streamName,
                                    Events = new MessagePack[] { item.Event as MessagePack }
                                };
                                await this.DoPublishEx(data, item.Stream, m =>
                                {
                                    m.Message.ReplayFor(this.Config.ConsumerSubscriptionId);
                                    m.Message.To(this.Config.Endpoint);
                                }, item.Task);
                            }
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        var ev = this._items.Take(token);
                        /// lastVersion doesnot work, for instance
                        /// we may have subscribed to multiple streams 
                        /// with different versions
                        /// 
                        if (1 == 1 || ev.Event.Version > this.lastVersion)
                        {
                            this.lastVersion = ev.Event.GetVersion();
                            await this.DoPublish(ev.Event, ev.Stream, m =>
                            {
                                m.Message.ReplayFor(this.Config.ConsumerSubscriptionId);
                                m.Message.To(this.Config.Endpoint);
                            }, ev.Task);
                        }
                    }
                }
            });
        }
    }
}
