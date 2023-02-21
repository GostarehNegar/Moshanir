using GN.Library.Data;
using GN.Library.Data.Deprecated;
using GN.Library.Data.Internal;
using Microsoft.AspNetCore.Hosting.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Data
{
    public interface IMessageRepository : IDocumentRepository<MessageContext>
    {
        Task<MessageContext> UpsertAsync(MessageContext context);
        void DeletAll();
    }
    class MessageRepository : DocumentRepository<MessageContext>, IMessageRepository
    {
        public MessageRepository(MessagingConfig cfg):base("")
        {
            this.connectionString = new DocumentStoreConnectionString
            {
                FileName = MessagingConstants.Instance.GetMessagingDbFileName(cfg.EndPointName)
            };
        }
        public MessageRepository(LiteDB.LiteDatabase db) : base(db)
        {
        }

        public Task<MessageContext> UpsertAsync(MessageContext context)
        {
            return Task.Run(() => 
            {
                return this.Upsert(context.Clone());
            });
        }
        public override MessageContext Upsert(MessageContext item)
        {
            return base.Upsert(item.Clone());
            
        }

        public void DeletAll()
        {
            this.GetCollection().Delete(x => true);
        }
    }
}
