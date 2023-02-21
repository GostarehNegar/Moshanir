using GN.Library.Messaging;
using System;
using System.Threading.Tasks;
using GN.Library.Messaging.Messages;

namespace GN.Library.Locks
{
    public class BusLock : ILock
    {
        private readonly IMessageBus bus;
        private readonly string key;
        private readonly int expiresMiliSeconds;
        private LockRecordReply _reply;

        public BusLock(IMessageBus bus, string key, int expiresMiliSeconds = 0)
        {
            this.bus = bus;
            this.key = key;
            this.expiresMiliSeconds = expiresMiliSeconds;
            this.Key = key;
        }
        public string Key { get; private set; }

        public bool Acquired => this._reply != null && this._reply.Acquired;
        public DateTime? ExpiresOn => this._reply?.ExpiresOn;

        public void Dispose(bool force)
        {
            this.Dispose();
            //_ = this.bus.GetResponse<UnLockRecordCommand, UnLockRecordReply>(new UnLockRecordCommand { Id = this.Key }, Throw:false);
        }

        public void Dispose()
        {
            //if (_reply != null)
            {
                _ = this.DisposeAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            //if (_reply != null)
            {
                await this.bus
                    .GetResponse<UnLockRecordCommand, UnLockRecordReply>
                        (new UnLockRecordCommand { Key = this.Key }, Throw: false);
            }

        }

        public async Task<bool> TryLock(int timeOut=0)
        {
            return (await Wait(timeOut)).Acquired;
        }

        public async Task<ILock> Wait(int timeOut=0)
        {
            this._reply = await this.bus
                    .GetResponse<LockRecordCommand, LockRecordReply>(new LockRecordCommand
                    {
                        Key = this.Key,
                        Expiration = this.expiresMiliSeconds,
                        Timeout = timeOut
                    },  Throw: false);
            return this;
        }
    }
}
