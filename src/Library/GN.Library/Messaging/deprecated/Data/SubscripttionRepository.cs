using GN.Library.Data;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using GN.Library.Data.Deprecated;
using GN.Library.Data.Internal;

namespace GN.Library.Messaging.Data
{
    public interface ISubscriptionRepository : IDocumentRepository<BusSubscription>
    {
        Task<BusSubscription> UpsertAsync(BusSubscription subscription, bool writeFile = true);
        Task<IEnumerable<BusSubscription>> GetItemsAsync(bool refresh = false);
        Task<IEnumerable<BusSubscription>> GetSubscriptions(MessageContext message);
        void Clear();
    }
    class SubscriptionRepository : DocumentRepository<BusSubscription>, ISubscriptionRepository
    {
        private List<BusSubscription> items;

        public SubscriptionRepository(MessagingConfig cfg) : base("")
        {
            this.connectionString = new DocumentStoreConnectionString
            {
                FileName = MessagingConstants.Instance.GetMessagingDbFileName(cfg.EndPointName)
            };
        }
        public SubscriptionRepository(LiteDatabase db) : base(db)
        {

        }

        public void Clear()
        {
            this.GetCollection().Delete(x => true);
        }

        public Task<IEnumerable<BusSubscription>> GetItemsAsync(bool refresh = false)
        {
            if (this.items == null || this.items.Count != this.GetCount() || refresh)
            {
                this.items = this.GetAll().ToList();
            }
            return Task.FromResult(this.items.AsEnumerable());
        }

        public async Task<IEnumerable<BusSubscription>> GetSubscriptions(MessageContext message)
        {
            var items = await this.GetItemsAsync();
            return items.Where(x => x.Matches(message));
        }

        public override BusSubscription Upsert(BusSubscription item)
        {
            var existing = this
                .GetFirstOrDefault(x => x.Topic == item.Topic && x.Endpoint == item.Endpoint && x.DynamicSelector == item.DynamicSelector);
            if (existing != null)
            {
                existing.AddHeaders(item.Headers);
            }
            else
            {
                existing = item;
            }
            existing.LastSeen = DateTime.UtcNow;
            return base.Upsert(existing);
        }
        public Task<BusSubscription> UpsertAsync(BusSubscription subscription, bool writeFile = true)
        {
            return Task.FromResult(this.Upsert(subscription));
        }

    }
}
