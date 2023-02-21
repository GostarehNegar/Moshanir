using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Redis
{
    public class RedisLock : IDisposable, ILock
    {
        private RedisServices redis;
        public bool IsDisposed { get; private set; }
        public bool IsDisposable { get; private set; }
        public bool Acquired { get; private set; }
        public string Key { get; private set; }

        internal RedisLock(string key, bool acquired, bool isDisposable, RedisServices redis)
        {
            this.Acquired = acquired;
            this.IsDisposable = isDisposable;
            this.redis = redis;
            this.Key = key;
            this.IsDisposed = false;
        }

        public void Dispose(bool force)
        {
            if (!IsDisposed)
            {
                if (force)
                {
                    this.redis.DeleteLock(this.Key);
                    this.IsDisposed = true;
                }
            }

        }
        public Task<RedisLock> Wait(int timeOut)
        {
            TaskCompletionSource<RedisLock> source = new TaskCompletionSource<RedisLock>();
            Task.Run(() =>
            {
                DateTime start = DateTime.UtcNow;
                RedisLock result = this;
                while (true)
                {
                    if (result.Acquired || (DateTime.UtcNow - start).TotalMilliseconds > timeOut)
                        break;
                    result = this.redis.Lock(this.Key, this.IsDisposable);
                    System.Threading.Thread.Sleep(10);
                }
                source.SetResult(result);
            });
            return source.Task;
        }

        public void Dispose()
        {
            Dispose(this.IsDisposable);
        }

       

        public Task<bool> TryLock(int timeOut = 120000)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            
            throw new NotImplementedException();
        }
    }


}
