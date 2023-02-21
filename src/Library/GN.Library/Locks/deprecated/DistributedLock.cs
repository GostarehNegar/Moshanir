using System;
using System.Threading.Tasks;

namespace GN.Library
{
    public class DistributedLock : IDisposable, ILock
    {
        private DistributedCacheLockManager manager;
        public bool IsDisposed { get; private set; }
        public bool IsDisposable { get; private set; }
        public bool Acquired { get; private set; }
        public string Key { get; private set; }

        internal DistributedLock(string key, bool acquired, bool isDisposable, DistributedCacheLockManager manager)
        {
            this.Acquired = acquired;
            this.IsDisposable = isDisposable;
            this.manager = manager;
            this.Key = key;
            this.IsDisposed = false;
        }

        public void Dispose(bool force)
        {
            if (!IsDisposed)
            {
                if (force)
                {
                    this.manager.DeleteLock(this.Key);
                    this.IsDisposed = true;
                }
            }

        }
        public Task<DistributedLock> Wait(int timeOut)
        {
            TaskCompletionSource<DistributedLock> source = new TaskCompletionSource<DistributedLock>();
            Task.Run(() =>
            {
                DateTime start = DateTime.UtcNow;
                DistributedLock result = this;
                while (true)
                {
                    if (result.Acquired || (DateTime.UtcNow - start).TotalMilliseconds > timeOut)
                        break;
                    result = this.manager.Lock(this.Key, this.IsDisposable);
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

        //async Task<ILock> Wait(int timeOut = 2 * 60 * 1000)
        //{
        //    var result = await this.Wait(timeOut);
        //    return result;
        //}

        public Task<bool> TryLock(int timeOut)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }

}
