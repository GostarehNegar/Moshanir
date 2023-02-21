using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Redis
{
    public interface IRedisServices
    {
        StackExchange.Redis.IDatabase GetDatabase(int dbNumber = -1);
        StackExchange.Redis.ISubscriber GetSubscriber();
        bool CheckConnection(int timeout);
       
        RedisLock Lock(string key, bool isdisposable = false, TimeSpan? expiration=null);
    }
}
