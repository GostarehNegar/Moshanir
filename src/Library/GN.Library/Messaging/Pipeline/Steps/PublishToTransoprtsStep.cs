using GN.Library.Messaging.Internals;
using GN.Library.Messaging.Transports;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace GN.Library.Messaging.Pipeline
{
    class PublishToTransoprtsStep : IPipelineStep
    {
        private readonly MessageBus bus;

        public PublishToTransoprtsStep(MessageBus bus)
        {
            this.bus = bus;
        }

        public async Task Handle(IPipelineContext context, Func<IPipelineContext, Task> next)
        {
            var options = context.MessageContext.Options();
            if (context.MessageContext.Message.Headers.ReplayRemainingCount().HasValue)
            {

            }

            if (!options.LocalOnly)
            {
                var handlers =
                    this.bus.GetTransports()
                    .Where(x => options.TransportMatch(x))
                    .Where(x => x.Matches(context.MessageContext))
                    .Select<IMessageTransport, Func<IMessageContext, Task>>(x => x.Publish)
                    .ToArray();
                await Task.WhenAny(
                        context.CancellationToken.AsTask(),
                        Task.WhenAll(
                            handlers.Select(x => x(context.MessageContext))));
            }
            await next(context);
        }
    }
}
