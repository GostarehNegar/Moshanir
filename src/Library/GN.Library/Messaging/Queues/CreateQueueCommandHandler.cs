using GN.Library.Shared.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Queues
{
    [MessageHandler(Topic = LibraryConstants.Subjects.Messaging.CreateQueue)]
    class CreateQueueCommandHandler : IMessageHandler<CreateQueueRequest>
    {
        public async Task Handle(IMessageContext<CreateQueueRequest> context)
        {
            var service = context.ServiceProvider.GetServiceEx<ILocalQueueService>();

            if (service != null && await context.Acquire())
            {
                try
                {
                    var name = context.Message?.Body?.Name;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        throw new Exception("Invalid Queue Name");
                    }
                    
                    var info = await service.GetQueueInformation(name);
                    if (info == null)
                    {
                        await service.CreateQueue(name);
                        info = await service.GetQueueInformation(name);
                    }
                    if (info == null)
                    {
                        throw new Exception("Failed to create queue");
                    }
                    await context.Reply(new CreateQueueReply { 
                    
                        Name = info.Name,
                        Info = new Shared.Messaging.MessagingQueueInformation
                        {
                            Id = info.Id,
                            Name = info.Name,
                            EndpointName = context.Bus.Advanced().EndpointName,
                        }
                    });

                }
                catch(Exception err)
                {
                    await  context.Reply(err);
                }


            }
        }
    }
}
