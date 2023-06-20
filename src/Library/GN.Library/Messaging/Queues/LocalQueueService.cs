using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using GN.Library.Shared.Messaging;
using System.Collections.Concurrent;
using GN.Library.ServiceDiscovery;
using GN.Library.Shared.ServiceDiscovery;

namespace GN.Library.Messaging.Queues
{
    public interface ILocalQueueService : IServiceDataProvider
    {
        string[] GetQueueNames(bool refersh = false);
        Task CreateQueue(string name);
        Task Enqueue(string queueName, MessagePack items);
        Task<MessagingQueueData> GetQueueInformation(string queueName);
        Task<IMessagingQueue> OpenQueue(string queueName);
        Task<MessagePack[]> Test(string queueName);
        bool HasQueue(string queueName);

    }
    class LocalQueueService : ILocalQueueService
    {
        private readonly MessagingQueueOptions options;
        private readonly IMessageBus bus;
        private ConcurrentDictionary<string, MessagingQueue> queues = new ConcurrentDictionary<string, MessagingQueue>();
        private string[] _names;
        public LocalQueueService(MessagingQueueOptions options, IMessageBus bus)
        {
            this.options = options;
            this.bus = bus;
        }

        public string[] GetQueueNames(bool refersh = false)
        {
            if (this._names == null || refersh)
            {
                var folder = this.options.GetFolderFullPath();
                this._names = Directory.EnumerateFiles(folder, "*.que", SearchOption.AllDirectories)
                    .Select(x => Path.GetFileNameWithoutExtension(x))
                    .ToArray();
            }
            return this._names;
        }
        public async Task CreateQueue(string name)
        {
            if (!HasQueue(name))
            {
                var queue = new LiteQueueRepository(this.options.GetQueueFullFileName(name));
                await queue.GetOrCreateQueueData(q => { q.Name = name; });
                queue.Dispose();
                this.GetQueueNames(true);
            }
        }
        public LiteQueueRepository GetQueueRepository(string name)
        {
            var fileName = this.options.GetQueueFullFileName(name);
            if (!File.Exists(this.options.GetQueueFullFileName(name)))
            {
                throw new FileNotFoundException(name);

            }
            return new LiteQueueRepository(this.options.GetQueueFullFileName(name));
        }



        public async Task Enqueue(string queueName, MessagePack items)
        {
            var queue = await this.OpenQueue(queueName);
            if (queue == null)
            {
                throw new Exception($"Queue Not Found. {queueName}");
            }
            if (queue != null)
            {
                await queue.Enqueue(items, default);
            }

            //return this.GetQueue(queueName).Enqueue(items);
        }

        public bool HasQueue(string queueName)
        {
            return this.GetQueueNames().Any(x => string.Compare(queueName, x, true) == 0);
        }
        public async Task<MessagingQueueData> GetQueueInformation(string queueName)
        {
            if (HasQueue(queueName))
            {
                using (var q = this.GetQueueRepository(queueName))
                {
                    return await q.GetOrCreateQueueData(null);
                }
            }
            return null;
        }

        public async Task<IMessagingQueue> OpenQueue(string queueName)
        {
            if (this.queues.TryGetValue(queueName, out var res))
            {
                return res;
            }
            if (!this.HasQueue(queueName))
            {
                var queue = new LiteQueueRepository(this.options.GetQueueFullFileName(queueName));
                await queue.GetOrCreateQueueData(q => { q.Name = queueName; });
                this.GetQueueNames(true);
                queue.Dispose();
            }
            var repo = new LiteQueueRepository(this.options.GetQueueFullFileName(queueName));
            var f = new MessagingQueue(repo, this.bus);
            this.queues.GetOrAdd(queueName, f);
            return f;

        }

        public async Task<MessagePack[]> Test(string queueName)
        {
            var items = new List<MessagePack>();
            var q = this.GetQueueRepository(queueName);

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await q.Enqueue(new MessagePack { });
                    await Task.Delay(10);
                }
            });
            while (items.Count < 100)
            {
                items.Add(await q.Dequeue(default));
            }
            return items.ToArray();

        }

        public ServiceData GetData()
        {
            var result = new ServiceData
            {
                Name = "queue",
                
            };
            result.Parameters["queues"] = this.GetQueueNames();
            return result;
        }
    }
}
