using GN.Library.Data;
using GN.Library.Data.Deprecated;
using GN.Library.Data.Internal;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Data
{
    public class AckData
    {
        public string Id { get; set; }
        public Guid MessageId { get; set; }
        public string EndPoint { get; set; }
        public bool Acknowledged { get; set; }
        public DateTime? LastAttempt { get; set; }
        public DateTime CreatedOn { get; set; }

    }
    public interface IAcknowledgeRepository
    {
        bool Acknowledge(Guid messageId, string endpoint, bool value);
        bool IsAcknowledged(Guid messageId, string endpoint);
        IEnumerable<AckData> GetAll();
        long Count();
        long AcknowledgedCount();
        long NotAcknowledgedCount();
        IEnumerable<AckData> GetAllNotAcknowledged();
        IEnumerable<AckData> GetPending(int? secondsFromLastAttempt);
        void Touch(AckData data);
        void Clear();
        void Cleanup();
    }
    class AcknowledgeRepository : DocumentRepository<AckData>, IAcknowledgeRepository
    {
        public AcknowledgeRepository(MessagingConfig cfg) : base("")
        {
            this.connectionString = new DocumentStoreConnectionString
            {
                FileName = MessagingConstants.Instance.GetMessagingDbFileName(cfg.EndPointName)
            };
        }
        public AcknowledgeRepository(LiteDatabase db) : base(db) { }

        public bool Acknowledge(Guid messageId, string endpoint, bool value)
        {
            if (IsAcknowledged(messageId, endpoint))
                return true;
            this.Upsert(new AckData
            {
                Id = $"{messageId}{endpoint}",
                Acknowledged = value,
                MessageId = messageId,
                EndPoint = endpoint,
                CreatedOn = DateTime.UtcNow,
                LastAttempt = DateTime.UtcNow,
            });
            return false;
        }

        public long AcknowledgedCount()
        {
            return this.LongCount(x => x.Acknowledged);
        }

        public void Cleanup()
        {
            this.GetCollection().Delete(x => x.Acknowledged && (DateTime.UtcNow - x.CreatedOn).TotalDays > 1);
        }

        public void Clear()
        {
            this.GetCollection().Delete(x => true);
        }

        public long Count()
        {
            return this.LongCount();
        }

        public IEnumerable<AckData> GetAllNotAcknowledged()
        {
            return this.Get(x => !x.Acknowledged);
        }

        public IEnumerable<AckData> GetPending(int? secondsFromLastAttempt)
        {
            secondsFromLastAttempt = secondsFromLastAttempt ?? 0;
            return this.Get(x => !x.Acknowledged && (x.LastAttempt == null || (DateTime.Now - x.LastAttempt.Value).TotalSeconds > secondsFromLastAttempt));
        }

        public bool IsAcknowledged(Guid messageId, string endpoint)
        {
            return this.GetCollection().FindById($"{messageId}{endpoint}")?.Acknowledged ?? false;
        }
        public long NotAcknowledgedCount()
        {
            return this.LongCount(x => !x.Acknowledged);
        }

        public void Touch(AckData data)
        {
            data.LastAttempt = DateTime.UtcNow;
            this.Upsert(data);
        }
    }
}
