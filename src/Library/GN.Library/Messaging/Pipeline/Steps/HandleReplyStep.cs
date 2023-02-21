using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Pipeline
{
    class HandleReplyStep : IPipelineStep
    {
        private readonly MessageBus bus;

        public HandleReplyStep(MessageBus bus)
        {
            this.bus = bus;
        }

        public Task Handle(IPipelineContext context, Func<IPipelineContext, Task> next)
        {
            var message = context.MessageContext;
            var requests = this.bus.Requests;

            if (message.IsReply() && message.Message.To() == this.bus.EndpointName)
            {
                var reply_to = message.Message.InReplyTo(null);
                if (reply_to != null &&
                requests.TryGetValue(reply_to, out var request) &&
                request.SetReply(message))
                {
                    requests.TryRemove(reply_to, out var _);
                }
            }
            if (message.IsAquire() && message.Message.To() == this.bus.EndpointName)
            {
                var reply_to = message.Message.InReplyTo(null);
                if (reply_to != null &&
                requests.TryGetValue(reply_to, out var request))
                {
                    return request.Acquire(message);

                }
            }
            return next(context);
        }
    }
}
