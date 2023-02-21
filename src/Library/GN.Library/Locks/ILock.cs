using System;
using System.Threading.Tasks;

namespace GN.Library
{
    public interface ILock : IDisposable,IAsyncDisposable
    {
        string Key { get; }
        /// <summary>
        /// True if the lock is successfully acquired.
        /// </summary>
        bool Acquired { get; }
        void Dispose(bool force);
        //Task<ILock> Wait(int timeOut=0);
        Task<bool> TryLock(int timeOut=0);
    }

}
