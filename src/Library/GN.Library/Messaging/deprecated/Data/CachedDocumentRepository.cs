using GN.Library.Data;
using GN.Library.Data.Internal;
using LiteDB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Data
{
    public interface ICachebleEntity<TId>
    {
        TId Id { get; set; }
    }

    class CachedDocumentRepository<TId, TEntity> : DocumentRepository<TId, TEntity> where TEntity : class, IGenericEntity<TId>
    {
        private ConcurrentDictionary<TId, TEntity> cache;
        private GN.Library.TaskScheduling.ActionQueueEx queue;

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            this.queue = new TaskScheduling.ActionQueueEx();
            return this.queue.StartAsync(cancellationToken);
        }
        public override TEntity Get(TId id)
        {
            TEntity result;
            if (!this.cache.TryGetValue(id, out result))
            {
                result = base.Get(id);
                if (result != null)
                {
                    this.cache.TryAdd(id, result);
                }
            }
            return result;
        }
        public override void Update(TEntity item)
        {
            this.cache.AddOrUpdate(item.Id, item, (k, i) => item);
            base.Upsert(item);
        }
        public override TEntity Upsert(TEntity item)
        {
            this.cache.AddOrUpdate(item.Id, item, (k, i) => item);
            return base.Upsert(item);
        }
        public override void Delete(TId id)
        {
            this.cache.TryRemove(id, out var val);
            base.Delete(id);
        }


        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            return this.cache == null ? Task.CompletedTask : this.queue.StopAsync(cancellationToken);
        }
        public CachedDocumentRepository(LiteDatabase db) : base(db)
        {
        }
        public CachedDocumentRepository(string connectionString = null) : base(connectionString)
        {
        }
        public CachedDocumentRepository(ILiteDbContext context) : base(context)
        {
        }

    }
}
