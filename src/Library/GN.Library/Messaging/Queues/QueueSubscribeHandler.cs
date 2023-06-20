using GN.Library.Shared.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Queues
{
    class QueueSubscribeHandler : IMessageHandler<QueueSubscribeRequest>
    {
        public async Task Handle(IMessageContext<QueueSubscribeRequest> context)
        {
            try
            {
                var message = context.Message.Body;
                var service = context.ServiceProvider
                    .GetServiceEx<ILocalQueueService>();
                if (service.HasQueue(message.QueueName))
                {
                    var queue = await service.OpenQueue(message.QueueName);
                    await queue.Subscribe(message.ConsummerId, cfg => {
                        cfg.Subject = message.Subject;
                        cfg.Endpoint = context.Message.From();
                    });

                }
            }
            catch (Exception err)
            {
                await context.Reply(err);
            }
        }
    }
}
