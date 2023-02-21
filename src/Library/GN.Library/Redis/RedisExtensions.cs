using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace GN.Library.Redis
{
    public static class RedisExtensions
    {
        private static RedisOptions _options;
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration, Action<RedisOptions> configure)
        {
            _options = _options ?? new RedisOptions();
            var options = _options;
            options.ConnectionString = configuration?.GetConnectionString("Redis");
            options.Validate();
            configure?.Invoke(options);
            options.Validate();
            if (!services.Any(x => x.ServiceType == typeof(RedisOptions)))
            {
                services.AddSingleton<RedisServices>();
                services.AddSingleton<IRedisServices>(s => s.GetServiceEx<RedisServices>());
                services.AddSingleton<IHealthCheck>(s => s.GetServiceEx<RedisServices>());
                //services.AddSingleton<ILockManager>(s => s.GetService<RedisServices>());
                services.AddSingleton(options);
                services.AddStackExchangeRedisCache(opt =>
                {
                    opt.Configuration = options.ConnectionString;
                    //opt.InstanceName

                });
            }
            return services;
        }
        public static void Subscibe<T>(this ISubscriber THIS , string channel, Action<T> cb)
        {
            THIS.Subscribe(channel, (_channel, value) =>
            {
                string _value = value;
                cb(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(_value));
            });
        }
        public static void Publish<T>(this ISubscriber THIS,string channel, T message)
        {
            var _value = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            THIS.Publish(channel, _value);
        }
    }
}
