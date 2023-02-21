using GN.Library.Messaging.Internals;
using GN.Library.Shared.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using GN.Library.Messaging.Messages;

namespace GN.Library.Messaging.Pipeline
{
    class PrePublishStep : IPipelineStep
    {
        public async Task Handle(IPipelineContext ctx, Func<IPipelineContext, Task> next)
        {
            var bus = ctx.Services.GetServiceEx<MessageBus>();
            if (ctx.MessageContext?.Message?.Subject == "$stream-data" && ctx.MessageContext.Message.To() == bus.EndpointName)
            {
                var msg = ctx.MessageContext.Cast<PublishStreamData>()?.Message;
                var data = msg?.Body;
                if (data != null && data.Events != null)
                {
                    var cnt = 0;
                    var remaining = msg.Headers.ReplayRemainingCount() + data.Events.Length;
                    foreach (var x in data.Events)
                    {
                        remaining--;
                        if (1 == 1)
                        {
                            var context = new MessageContext<object>(LogicalMessage.Unpack(x), null, bus);
                            context.Message.SetTopic(MessageTopic.Create(context.Message.Subject, data.Stream, context.Message.Version));
                            //context.Message.Headers.Clear();
                            context.Message.ReplayFor(msg.ReplayFor());
                            context.Message.Headers.ReplayRemainingCount(remaining);
                            context.Message.To(msg.To());
                            context.Message.Headers.StreamEndpoint(msg.From());
                            context.Message.Headers.ReplayTo(msg.To());
                            context.LocalOnly(true);
                            await context.Publish();
                        }
                        else
                        {
                            var result = bus.CreateMessage(x)
                                .UseTopic(x.Subject, data.Stream, x.Version)
                                .LocalOnly(true)
                                .Cast<object>();
                            result.Message.ReplayFor(msg.ReplayFor());
                            result.Message.Headers.ReplayRemainingCount(remaining);
                            result.Message.To(msg.To());
                            await result.Publish();
                        }
                    }
                }

            }
            await next(ctx);
        }
    }
}
