using GN.Library.Shared;
using GN.Library;
using GN.Library.Locks;
using GN.Library.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN
{
    public static partial class Extensions
    {
        public static IServiceCollection AddLockServices(this IServiceCollection services)
        {
            services.AddSingleton<DistributedLockService>();
            services.AddTransient<IHostedService>(sp => sp.GetServiceEx<DistributedLockService>());
            return services;
        }
        public static ILock GetLock(this IMessageContext ctx, string id = null, int expiresInMiliseconds = 0)
        {
            id = id ?? ctx.Message.MessageId.ToString();
            var result = new BusLock(ctx.Bus, id, expiresInMiliseconds);
            return result;
        }
        /// <summary>
        /// Gets a distributed lock object that can be used to lock an object using its key.
        /// The lock will be retained for the specified duration or may be disposed manually.
        /// 
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="key"> The key identifier for lock.  </param>
        /// <param name="owner"> The owner or reason of lock. </param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public static ILock GetLock(this IMessageBus bus, string key, int expiration = 60 * 1000)
        {
            //var _lock =  new BusLock(bus, key,   expiration);
            return new BusLock(bus, key, expiration);

        }

    }
}
