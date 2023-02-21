using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace GN.Library.Messaging.Pipeline
{
    class PublishToSubscribersStep : IPipelineStep
    {
        private readonly MessageBus bus;

        public PublishToSubscribersStep(MessageBus bus)
        {
            this.bus = bus;
        }

        public async Task Handle(IPipelineContext context, Func<IPipelineContext, Task> next)
        {
            var handlers = this.bus.Subscriptions.Query(context.MessageContext)
               .Select<IMessageBusSubscription, Func<IMessageContext, Task>>(x => x.Handle)
               .ToArray();
            await Task.WhenAny(
                context.CancellationToken.AsTask(),
                Task.WhenAll(handlers
                    .Select(x => x(context.MessageContext))));
            await next(context);
        }
    }
}
