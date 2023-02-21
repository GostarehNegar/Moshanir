using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Helpers
{
    public class RecordLock : IDisposable
    {
        //public int Counter { get; private set; }
        public string Id { get; private set; }

        public RecordLock(string key)
        {
            this.Id = key;
        }
        
        public bool Expired()
        {
            return false;
        }

        public void Dispose()
        {
            RecordLocker.Remove(this);
        }
    }
    public static class RecordLocker
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<string, RecordLock> locks
            = new ConcurrentDictionary<string, RecordLock>();
        public static RecordLock TryLock(Guid id)
        {
            //await Task.CompletedTask;
            RecordLock result = null;
            if (locks.TryGetValue(id.ToString(), out var _lock))
            {
               // _lock.Touch();
                if (_lock.Expired())
                {
                    locks.TryRemove(id.ToString(), out var _);
                }
            }
            else 
            {
                result = new RecordLock(id.ToString());
                locks.TryAdd(id.ToString(), result);
            }
            return result;
        }

        public static bool IsLocked(Guid id)
        {
            return locks.TryGetValue(id.ToString(), out var _);
        }



        internal static void Remove(RecordLock @lock)
        {
            locks.TryRemove(@lock.Id, out var _);
        }


    }
}
