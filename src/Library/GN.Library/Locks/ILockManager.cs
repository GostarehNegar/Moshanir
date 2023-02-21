using System;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace GN.Library
{
    public interface ILockManager
    {
        ILock Lock(string key, bool isdisposable = false, TimeSpan? expiration = null);
    }
    internal interface IDeleteLock
    {
        void DeleteLock(string key);
    }

}
