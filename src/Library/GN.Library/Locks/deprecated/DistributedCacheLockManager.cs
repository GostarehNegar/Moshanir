using GN.Library.Redis;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GN.Library
{
    class DistributedCacheLockManager : ILockManager, IDeleteLock
    {
        private IDistributedCache chache => lazy_chache.Value;
        private readonly IRedisServices redis;
        private readonly Lazy<IDistributedCache> lazy_chache;



        public DistributedCacheLockManager(IServiceProvider serviceProvider, IRedisServices redis)
        {
            this.redis = redis;
            var caches = serviceProvider.GetServiceEx<IEnumerable<IDistributedCache>>();
            var redisCache = caches.FirstOrDefault(x => x.GetType().Name.ToLower().Contains("redis"));
            var memory = caches.FirstOrDefault(x => !x.GetType().Name.ToLower().Contains("redis"));
            this.lazy_chache = new Lazy<IDistributedCache>(() =>
            {
                return ((redis.CheckConnection(2 * 1000)) ? redisCache : memory) ?? caches.FirstOrDefault();

            }, true);

        }

        public DistributedLock Lock(string key, bool autoDispose = false, TimeSpan? expiration = null)
        {
            var signature = "locks:";
            bool flag = false;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (!key.StartsWith(signature))
                key = signature + key;
            if (!expiration.HasValue)
                expiration = TimeSpan.FromDays(1);
            try
            {
                flag = string.IsNullOrWhiteSpace(this.chache.GetString(key));
                if (flag)
                {
                    this.chache.SetString(key, "True", new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiration
                    });
                }
            }
            catch (Exception err)
            {
                flag = true;

            }
            return new DistributedLock(key, flag, autoDispose, this);

        }
        public void DeleteLock(string key)
        {
            this.chache.Remove(key);
        }

        ILock ILockManager.Lock(string key, bool autoDispose, TimeSpan? expiration)
        {
            return this.Lock(key, autoDispose, expiration);
        }
    }

}
