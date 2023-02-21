using GN.Library.Data;
using GN.Library.Data.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Data
{
	public interface IMessageQueueRepository
	{
		MessageContext GetOrAdd(MessageContext message);
		MessageContext TryGet(Guid Id);
		bool IsProcessedByMe(Guid messageId);
		Task CleanUp(bool force = false);
		void DeleteAll();

	}
	public class MessageQueueData
	{
		public Guid Id { get; set; }
		public DateTime CreatedOn { get; set; }
	}
	class MessageQueueRepository : DocumentRepository<Guid, MessageQueueData>, IMessageQueueRepository
	{
		private static ConcurrentDictionary<Guid, MessageContext> cache = new ConcurrentDictionary<Guid, MessageContext>();
		private DateTime lastCleanup;

		public ConcurrentDictionary<Guid, MessageContext> Cache => cache;
		public MessageQueueRepository(MessagingConfig cfg) : base("")
		{
			this.connectionString = new DocumentStoreConnectionString
			{
				FileName = MessagingConstants.Instance.GetMessagingDbFileName(cfg.EndPointName)
			};
		}
		public MessageQueueRepository(LiteDB.LiteDatabase db) : base(db)
		{
		}

		public MessageQueueRepository(string connectionString = null) : base(connectionString)
		{
		}

		public bool IsProcessedByMe(Guid messageId)
		{
			return this.Cache.ContainsKey(messageId) || this.Exists(messageId);
		}
		public MessageContext GetOrAdd(MessageContext message)
		{

			MessageContext result = message;
			if (result != null)
			{
				if (!this.Cache.TryGetValue(message.Id, out result))
				{
					this.Cache.TryAdd(message.Id, message);
					result = message;

				}
				if (!Exists(message.Id) && 1 == 0)
				{
					this.Upsert(new MessageQueueData
					{
						Id = message.Id,
						CreatedOn = DateTime.UtcNow
					});
				}
			}
			return result;
		}

		public Task CleanUp(bool force = false)
		{
			var result = Task.CompletedTask;
			if ((DateTime.Now - lastCleanup).TotalMinutes > 30 || force)
			{
				this.lastCleanup = DateTime.Now;
				result = Task.Run(() =>
				{
					foreach (var item in Cache.Values)
					{
						this.Upsert(new MessageQueueData
						{
							Id = item.Id,
							CreatedOn = DateTime.UtcNow
						});
					}
					foreach (var item in Cache.Values.Where(x => (DateTime.UtcNow - x.GetPublishedDate()).TotalHours > 1))
					{
						Cache.TryRemove(item.Id, out var r);
					}
					this.Delete(x => (DateTime.UtcNow - x.CreatedOn).TotalDays > 1);
				});
			}
			return result;
		}

		public void DeleteAll()
		{
			cache = new ConcurrentDictionary<Guid, MessageContext>();
			this.Delete(x => true);
		}

		public MessageContext TryGet(Guid Id)
		{
			return this.Cache.TryGetValue(Id, out var m)
				? m
				: null;
		}
	}
}
