
using GN.Library.Shared.Deprecated;
using GN.Library.Messaging;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching;
using Microsoft.Extensions.Caching.Memory;
using GN.Library.Messaging.Messages;

namespace GN.Library.Locks
{

    class DistributedLockService : IHostedService
    {
        class LockData
        {
            public string Key { get; set; }
            public string Owner { get; set; }
            public DateTime ExpiresOn { get; set; }
            public int TimeOut { get; set; }

        }
        private readonly IMessageBus bus;
        private readonly IMemoryCache chache;
        private const int DefaultExpiration = 5 * 60 * 1000;
        public DistributedLockService(IMessageBus bus, IMemoryCache chache)
        {
            this.bus = bus;
            this.chache = chache;
        }
        private async Task Handle(IMessageContext<LockRecordCommand> ctx)
        {
            var msg = ctx.Message.Body;
            
            LockRecordReply reply = new LockRecordReply
            {
                Key = msg.Key,
            };
            if (!this.chache.TryGetValue<LockData>(msg.Key, out var _lock))
            {
                _lock = _lock ?? new LockData
                {
                    Key = msg.Key,
                    TimeOut = msg.Expiration > 10 ? msg.Expiration : DefaultExpiration,
                    ExpiresOn = DateTime.UtcNow.AddMilliseconds(msg.Expiration > 10 ? msg.Expiration : DefaultExpiration),

                };
                this.chache.Set(msg.Key, _lock, TimeSpan.FromMilliseconds(msg.Expiration));
                reply = new LockRecordReply
                {
                    Key = _lock.Key,
                    ExpiresOn = _lock.ExpiresOn,
                    Acquired = true
                };
            }
            else
            {
                int waited = 0;
                LockData __lock = _lock;
                while (waited < msg.Timeout && this.chache.TryGetValue<LockData>(msg.Key, out __lock))
                {
                    await Task.Delay(100);
                    waited += 100;
                    
                }
                if (!this.chache.TryGetValue<LockData>(msg.Key, out __lock))
                {
                    __lock = new LockData
                    {
                        Key = msg.Key,
                        TimeOut = msg.Expiration > 10 ? msg.Expiration : DefaultExpiration,
                        ExpiresOn = DateTime.UtcNow.AddMilliseconds(msg.Expiration > 10 ? msg.Expiration : DefaultExpiration),
                        

                    };
                    this.chache.Set(msg.Key, __lock, TimeSpan.FromMilliseconds(msg.Expiration));
                    reply = new LockRecordReply
                    {
                        Key = __lock.Key,
                        ExpiresOn = __lock.ExpiresOn,
                        Acquired = true
                    };
                }
                else
                {
                    reply = new LockRecordReply
                    {
                        Key = _lock.Key,
                        ExpiresOn = _lock.ExpiresOn,
                        Acquired = false
                    };
                }

            }
            await ctx.Reply(reply);

        }
        private async Task Handle(IMessageContext<UnLockRecordCommand> ctx)
        {
            var msg = ctx.Message.Body;
            this.chache.Remove(msg.Key);
            await ctx.Reply(new UnLockRecordCommand { Key = msg.Key });
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.bus.CreateSubscription<LockRecordCommand>(Handle)
                .Subscribe();

            await this.bus.CreateSubscription<UnLockRecordCommand>(Handle)
                .Subscribe();

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

        }
    }
}
