using GN.Library.Shared.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using GN.Library.Shared.Messaging;

namespace GN.Library.Messaging.Queues
{
    class GetQueuesInformationHandler : IMessageHandler<GetQueuesNamesRequest>
    {
        public GetQueuesInformationHandler()
        {

        }

        public IServiceProvider ServiceProvider { get; }

        public async Task Handle(IMessageContext<GetQueuesNamesRequest> context)
        {
            var service = context.ServiceProvider.GetServiceEx<ILocalQueueService>();

            if (service != null)
            {
                await context.Reply(
                    new GetQueuesNamesReply
                    {
                        Queues = (service.GetQueueNames() ?? new string[] { })
                            .ToArray()
                    });
            }
        }
    }
}
