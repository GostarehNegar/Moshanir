using GN.Library.Natilus.Internals;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Natilus.Messaging.Internals
{
    public enum SubscriptionStrategy
    {
        Push,
        Pull,
        JetStreamPush,
        JetStreamPull,
        JetStreamFetch,
    }
    class NatilusSubscription : INatilusSubscription, INatilusSubscriptionBuilder
    {
        private readonly NatilusBus bus;
        public string Subject { get; set; }
        public string Queue { get; set; }
        public SubscriptionStrategy Strategy { get; set; }
        private ISubscription subscription;
        private MessageHandler Handler { get; set; }

        public NatilusSubscription(NatilusBus bus)
        {
            this.bus = bus;
        }

        public async Task<INatilusSubscription> Build()
        {
            await this.bus.NatilusSubscribe(this);
            return this;
        }

        public INatilusSubscriptionBuilder WithHandler(MessageHandler handler)
        {
            this.Handler = handler;
            return this;
        }

        internal void HandleMsg(NatilusBus bus, Msg message, INatilusSerializer serializer)
        {
            var ctx = new NatilusMessageContext(bus, new NatilusMessage(message, serializer));
            this.Handler?.Invoke(ctx);
        }

        public INatilusSubscriptionBuilder WithSubject(string subject)
        {
            this.Subject = subject;
            return this;
        }
        internal async Task DoSubscribe(NatilusBus bus, IConnection connection, INatilusSerializer serializer)
        {
            await Task.CompletedTask;
            switch (this.Strategy)
            {
                default:
                    {
                        this.subscription = connection
                            .SubscribeAsync(this.Subject, this.Queue, (s, e) => this.HandleMsg(bus, e.Message, serializer));
                    }
                    break;
            }
        }

        public INatilusSubscriptionBuilder WithAsyncHandler(MessageHandlerAsync handler)
        {
            this.Handler = ctx =>
            {
                if (handler != null)
                {
                    handler(ctx).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            };
            return this;
        }
    }
}
