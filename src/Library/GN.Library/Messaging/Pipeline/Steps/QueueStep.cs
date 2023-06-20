using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Pipeline
{
    class QueueStep : IPipelineStep
    {

        public QueueStep()
        {

        }
        public QueueStep init()
        {
            return this;
        }
        public async Task Handle(IPipelineContext ctx, Func<IPipelineContext, Task> next)
        {
            var message = ctx?.MessageContext?.Message;
            var bus = ctx.Services.GetServiceEx<IMessageBus>();
            if (message != null && !string.IsNullOrWhiteSpace(ctx.MessageContext.Options().QueueName)) // && message.Version == null)
            {
                await ctx.MessageContext.Bus.Advanced().Enqueue(ctx.MessageContext);
                return;
            }
            if (message != null && message.IsQueuedMessage() && message.To() == bus.Advanced().EndpointName)
            {
                var pack = ctx.MessageContext?.Message.GetBody<QueueMessage>();
                var _ctx = bus.Advanced()
                    .CreateMessageContext(pack.Pack)
                    .WithOptions(opt => opt.WithBypassDuplicateValidations(true));
                _ctx.QueueMessage(ctx.MessageContext);
                _ctx.Message.ReplayFor(message.ReplayFor());
                //_ctx.Message.Headers.ReplayTo(message.To());
                //_ctx.GetProperty<Func<Task>>("$ack", () => () => ctx.MessageContext.Reply("ack"));
                await _ctx.Publish();
            }
            await next(ctx);
        }


    }
}
