using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace GN.Library.Messaging.Queues
{
    public interface IMessagingQueue
    {
        Task Enqueue(MessagePack message, CancellationToken cancellationToken);
        Task Subscribe(string Id, Action<QueueSubscriber> configure);
    }
    public class QueueSubscriber
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string Endpoint { get; set; }
        internal long Count { get; set; }
    }
    class MessagingQueue : IMessagingQueue
    {
        private readonly IQueueRepository repository;
        private readonly IMessageBus bus;
        private ConcurrentDictionary<string, QueueSubscriber> subscriptions = new ConcurrentDictionary<string, QueueSubscriber>();
        private bool _started;
        public MessagingQueue(IQueueRepository repository, IMessageBus bus)
        {
            this.repository = repository;
            this.bus = bus;
        }

        public Task Enqueue(MessagePack message, CancellationToken cancellationToken)
        {
            return this.repository.Enqueue(message, cancellationToken);
        }

        public async Task Start(CancellationToken cancellationToken)
        {

            this._started = true;
            await Task.CompletedTask;
            while (!cancellationToken.IsCancellationRequested)
            {
                var item = await this.repository.Dequeue(cancellationToken);
                /// dequeue 1 item
                /// publish and wait for ack
                /// 
                bool handled = false;
                foreach (var sub in this.subscriptions.Values.OrderBy(x => x.Count))
                {
                    var _ctx = this.bus.CreateMessage(new QueueMessage { Pack = item })
                        .UseTopic(LibraryConstants.Subjects.Messaging.QueueMessage)
                        .WithMessage(m =>
                        {
                            m.Headers.AddFlag(Internals.MessageFlags.QueuedMessage);
                            m.ReplayFor(sub.Id);
                            m.To(sub.Endpoint);
                        });
                    var res = await _ctx.CreateRequest()
                        .WaitFor(x => true, cancellationToken)
                        .TimeOutAfter(LibraryConstants.DefaultTimeout, cancellationToken, throwIfTimeOut: false);
                    if (res != null)
                    {
                        sub.Count++;
                        handled = true;
                    }
                }
                if (!handled)
                {
                    await this.repository.Enqueue(item, cancellationToken);
                    break;
                }
            }
            this._started = false;

        }

        public async Task Subscribe(string Id, Action<QueueSubscriber> configure)
        {
            await Task.CompletedTask;
            var sub = new QueueSubscriber
            {
                Id = Id
            };
            configure?.Invoke(sub);
            this.subscriptions.GetOrAdd(Id, sub);
            if (!this._started)
            {
                _ = this.Start(this.bus.Advanced().CancellationToken);
            }
        }
    }
}
