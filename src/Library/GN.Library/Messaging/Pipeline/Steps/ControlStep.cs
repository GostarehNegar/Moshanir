using GN.Library.Contracts_Deprecated;
using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using GN.Library.Messaging.Messages;

namespace GN.Library.Messaging.Pipeline
{
    class ControlStep:IPipelineStep
    {
        private readonly MessageBus bus;

        public ControlStep(MessageBus bus)
        {
            this.bus = bus;
        }

        public async Task Handle(IPipelineContext context, Func<IPipelineContext, Task> next)
        {
            var message = context.MessageContext?.Message;
            if (message.Subject == MessageTopicHelper.GetTopicByType(typeof(QueryHandler)))
            {
                var _message = message.Cast<QueryHandler>();
                if (_message != null)
                {
                    var messageContext = this.bus.CreateContext(MessageTopic.Create(_message.Body.TopicName, _message.Stream), "query");
                    var handler = this.bus.Subscriptions.Query(messageContext);
                    if (handler.Count() > 0)
                    {
                        await context.MessageContext.Reply(new QueryHandlerReply { EndpointName = this.bus.Options.GetEndpointName(), IsReady = true });
                    }
                }
            }
            var to = context.MessageContext.Message?.Headers.To();
            bool toMe = string.IsNullOrWhiteSpace(to) || string.Compare(to, context.MessageContext.Bus.Advanced().EndpointName, true) == 0;
            if (toMe && message?.Subject == MessageTopicHelper.GetTopicByType(typeof(PingBus)))
            {
                await context.MessageContext.Reply(new PingBusReply
                {
                    Name = context.MessageContext.Bus.Advanced().EndpointName
                });
            }
            await next(context);
        }
    }
}
