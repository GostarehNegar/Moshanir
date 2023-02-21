using GN.Library.Shared;
using GN.Library.Messaging.Internals;
using GN.Library.Messaging.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Pipeline
{
    class SaveToStreamStep : IPipelineStep
    {

        public SaveToStreamStep()
        {

        }
        public SaveToStreamStep init()
        {
            return this;
        }
        public async Task Handle(IPipelineContext ctx, Func<IPipelineContext, Task> next)
        {
            var message = ctx?.MessageContext?.Message;
            if (message != null && !string.IsNullOrWhiteSpace(message.Stream) && message.Version == null)
            {
                await ctx.MessageContext.Bus.Advanced().SaveToStream(message, false);
                return;
            }
            await next(ctx);
        }
    }
}
