using GN.Library.Shared.Messaging;
using GN.Library.Shared.Messaging.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using GN.Library.Messaging.Internals;

namespace GN.Library.Messaging.Queues
{
    public interface IQueueManagerService
    {
        Task<MessagingQueueInformation> GetQueue(string name, bool create, CancellationToken cancellationToken);
        Task Enqueue(IMessageContext context, CancellationToken cancellationToken);
        Task Subscribe(IMessageBusSubscription subscriber, CancellationToken cancellationToken);
    }
    class QueueManagerService : IQueueManagerService
    {
        private readonly IMessageBus bus;
        public ConcurrentDictionary<string, MessagingQueueInformation> queueData = new ConcurrentDictionary<string, MessagingQueueInformation>();

        public QueueManagerService(IMessageBus bus)
        {
            this.bus = bus;
        }

        public async Task Enqueue(IMessageContext context, CancellationToken cancellationToken)
        {
            var queue = context.Options().QueueName;// context?.Message?.Headers.Queue();
            if (string.IsNullOrEmpty(queue))
            {
                throw new Exception("Invalid Queue");
            }
            var data = await this.GetQueue(queue, true, cancellationToken);
            if (data == null)
            {
                throw new Exception($"Queue Not Found {queue}");
            }
            var res = await this.bus.Rpc.Call<EnqueueRequest, EnqueueReply>(new EnqueueRequest
            {
                Item = context.Message.Pack(),
                QueueName = queue
            });

        }

        public async Task<MessagingQueueInformation> GetQueue(string name, bool create, CancellationToken cancellationToken)
        {
            if (queueData.TryGetValue(name, out var result))
            {
                return result;
            }
            await this.bus.CreateMessage(new GetQueuesNamesRequest { })
                .CreateRequest()
                .WaitAll(res =>
                {
                    foreach (var item in res)
                    {
                        var reply = item.Message?.GetBody<GetQueuesNamesReply>();
                        if (reply != null && reply.Queues != null)
                        {
                            reply.Queues.ToList()
                            .ForEach(x =>
                            {
                                var info = new MessagingQueueInformation
                                {
                                    Name = x,
                                    EndpointName = item.Message.Headers.From()
                                };
                                //this.queueData.AddOrUpdate(x, info, (a, b) => { return info; });
                                this.queueData.GetOrAdd(x, info);
                            });
                        }
                    }
                    return true;
                }, 5000);
            if (queueData.TryGetValue(name, out result))
            {
                return result;
            }
            try
            {
                var res = await this.bus.Rpc.Call<CreateQueueRequest, CreateQueueReply>(new CreateQueueRequest { Name = name });
                if (res != null && !string.IsNullOrWhiteSpace(res.Name))
                {
                    this.queueData.GetOrAdd(res.Name, res.Info);
                }
            }
            catch (Exception err)
            {
                throw;
            }
            return this.queueData.TryGetValue(name, out var _r) ? _r : null;
        }

        public Task Start(string queueName, IMessageBusSubscription subscriber)
        {
            throw new NotImplementedException();
        }

        public async Task Subscribe(IMessageBusSubscription subscriber, CancellationToken cancellationToken)
        {
            var queue = subscriber.QueueName;
            if (!string.IsNullOrWhiteSpace(queue))
            {
                if (await this.GetQueue(queue, true, cancellationToken) == null)
                {
                    throw new Exception($"Failed to create queue");
                }
                var res = await this.bus.Rpc
                    .Call<QueueSubscribeRequest, QueueSubscribeResponse>(new QueueSubscribeRequest
                    {
                        ConsummerId = subscriber.Properties.RemoteId(null) ?? subscriber.Id.ToString(),
                        Subject = subscriber.Topic?.Subject,
                        QueueName = queue
                    });
            }
        }
    }
}
