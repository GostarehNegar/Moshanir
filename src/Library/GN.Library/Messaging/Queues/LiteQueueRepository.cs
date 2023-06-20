using GN.Library.Data.Lite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using LiteDB;
using GN.Library.Shared.Messaging;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;

namespace GN.Library.Messaging.Queues
{

    public interface IQueueRepository
    {
        IEnumerable<MessagePack> GetPacks(CancellationToken cancellationToken);
        Task<MessagePack> Dequeue(CancellationToken cancellationToken = default);
        Task Enqueue(MessagePack item, CancellationToken cancellationToken = default);
    }
    class QueueItemData
    {
        public long Id { get; set; }
        public MessagePack Pack { get; set; }
    }
    class LiteQueueRepository : LiteDatabaseEx, IQueueRepository, IDisposable
    {
        private BlockingCollection<MessagePack> block = new BlockingCollection<MessagePack>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);


        public LiteQueueRepository(string connectionString) : base(connectionString)
        {
            
        }
        public IEnumerable<MessagePack> GetPacks(CancellationToken cancellationToken)
        {
            return this.block.GetConsumingEnumerable(cancellationToken);
        }
        public async Task<MessagingQueueData> GetOrCreateQueueData(Action<MessagingQueueData> fillData)
        {
            using (var db = await this.Lock(true, default))
            {
                var col = db.GetCollection<MessagingQueueData>();
                var result = col.FindAll().FirstOrDefault();
                if (result == null && fillData != null)
                {
                    result = new MessagingQueueData();
                    fillData.Invoke(result);
                    col.Upsert(result);
                }
                result.ItemsCount = db.GetCollection<MessagePack>().LongCount();
                return result;
            }
        }
        public async Task Enqueue(MessagePack item, CancellationToken cancellationToken = default)
        {
            using (var db = await this.Lock(true, default))
            {
                var col = db.GetCollection<QueueItemData>();
                col.Insert(new QueueItemData { Pack = item });
                _signal.Release();
            }
        }

        public async Task<MessagePack> Dequeue(CancellationToken cancellationToken, int trial)
        {
            using (var db = await this.Lock(true, cancellationToken))
            {
                var col = db.GetCollection<QueueItemData>();
                var item = col.Query().FirstOrDefault();
                if (item != null)
                {
                    col.Delete(item.Id);
                    return item?.Pack;
                }
            }
            if (trial > 3)
            {
                throw new Exception("Unexpected!!!");
            }
            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
            return await Dequeue(cancellationToken, trial++);

        }
        public Task<MessagePack> Dequeue(CancellationToken cancellationToken)
        {
            return Dequeue(cancellationToken, 0);
        }

        public void Dispose()
        {
            this._signal.Dispose();
        }
    }
}
