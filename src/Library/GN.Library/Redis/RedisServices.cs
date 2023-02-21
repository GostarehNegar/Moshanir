using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Redis
{
    class RedisServices : IRedisServices, IHealthCheck, ILockManager
    {
        private static ConnectionMultiplexer redis;
        private static object _lock = new object();
        private readonly RedisOptions options;
        const string signature = "lock/";
        public RedisServices(RedisOptions options)
        {
            this.options = options;
        }
        internal ConnectionMultiplexer GetRedis(bool refersh = false, int connectTimeOut = 2 * 1000)
        {
            if (redis == null || refersh)
            {
                lock (_lock)
                {
                    if (redis == null)
                    {

                        redis = ConnectionMultiplexer.Connect(options.GetMultiplexreConnectionString(), cfg =>
                        {
                            cfg.ConnectTimeout = connectTimeOut;
                            cfg.AbortOnConnectFail = false;
                        });
                        

                    }
                }
            }
            return redis;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            if (this.CheckConnection(2000))
            {
                return context.Healthy("Redis Services")
                    .WriteLine($"Server: {this.options.GetMultiplexreConnectionString()}");
            }
            else
            {
                return context.Unhealthy("Redis Services")
                    .WriteLine($"Server {this.options.GetMultiplexreConnectionString()}");
            }
        }

        public IDatabase GetDatabase(int dbNumber = -1)
        {

            return GetRedis().GetDatabase(dbNumber);
        }

        public ISubscriber GetSubscriber()
        {
            return GetRedis().GetSubscriber();
        }

        public bool CheckConnection(int timeout)
        {
            return GetRedis(connectTimeOut: timeout).IsConnected;
        }

        public RedisLock LockJob(string key)
        {
            //if (string.IsNullOrWhiteSpace(key))
            //    throw new ArgumentNullException(nameof(key));
            //key = string.Format("{0}/{1}", signature, key);

            return Lock(key, false);
        }
        public RedisLock LockRecord(string key)
        {
            //if (string.IsNullOrWhiteSpace(key))
            //    throw new ArgumentNullException(nameof(key));
            //key = string.Format("{0}/{1}", signature, key);
            return Lock(key, true);
        }
        public RedisLock Lock(string key, bool isdisposable = false, TimeSpan? expiration = null)
        {
            bool flag = false;
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (!key.StartsWith(signature))
                key = signature + key;
            if (!expiration.HasValue)
                expiration = TimeSpan.FromDays(1);
            try
            {
                flag = this.GetDatabase().StringSet(key, "True", expiration, When.NotExists);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Acquire lock fail...{ex.Message}");
                flag = true;
            }
            return new RedisLock(key, flag, isdisposable, this);
        }
        public bool DeleteLock(string key)
        {
            bool flag = false;
            try
            {
                flag = this.GetDatabase().KeyDelete(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Acquire lock fail...{ex.Message}");
                flag = true;
            }
            return flag;

        }

        ILock ILockManager.Lock(string key, bool autoDispose=false, TimeSpan? expiration=null)
        {
            return this.Lock(key, autoDispose, expiration);
        }
    }
}
