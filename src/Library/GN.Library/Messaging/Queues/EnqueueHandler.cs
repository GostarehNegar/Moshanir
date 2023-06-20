using GN.Library.Shared.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Queues
{
    class EnqueueHandler : IMessageHandler<EnqueueRequest>
    {
        public async Task Handle(IMessageContext<EnqueueRequest> context)
        {
            try
            {
                var message = context.Message.Body;
                var service = context.ServiceProvider.GetServiceEx<ILocalQueueService>();
                if (!string.IsNullOrWhiteSpace(message.QueueName) && service != null && service.HasQueue(message.QueueName))
                {
                    await (await service.OpenQueue(message.QueueName)).Enqueue(message.Item, context.CancellationToken);
                    await context.Reply(new EnqueueReply
                    {
                        QueueName = message.QueueName,
                        Message = message.Item
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
