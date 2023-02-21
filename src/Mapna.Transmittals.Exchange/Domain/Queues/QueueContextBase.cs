using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Mapna.Transmittals.Exchange.Internals;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mapna.Transmittals.Exchange.Services.Queues
{
    public class QueueContextBase : IDisposable
    {
        protected IServiceScope scope;
        protected IServiceProvider serviceProvider;
        public IServiceProvider ServiceProvider => serviceProvider ?? scope?.ServiceProvider;
        protected CancellationTokenSource cancellation;
        public CancellationToken CancellationToken => cancellation.Token;
        private ConcurrentDictionary<string, object> dic = new ConcurrentDictionary<string, object>();
        private ILogger logger;
        public ILogger GetLogger()
        {
            logger = logger ?? ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("CTX");
            return logger;
        }
        public void Dispose()
        {
            scope?.Dispose();
            cancellation?.Dispose();
        }
        public void Cancel()
        {
            cancellation?.Cancel();
        }
        public int MaxTrials { get; set; }
        public int Trial { get; set; }
        public bool IsRetryable(Exception exception)
        {
            return exception.IsRetryable() && Trial < MaxTrials;
        }

        public virtual int IncrementTrials()
        {
            Trial++;
            //if (this.Trial > MaxTrials)
            //{
            //    return -1;
            //}
            return Trial * Trial * 6 * 1000;
        }
        internal T GetCachedService<T>(TimeSpan timeSpan = default, Func<IServiceProvider, T> constructor = null)
        {
            var name = typeof(T).FullName;
            constructor = constructor ?? (sp => sp.GetService<T>());
            if (timeSpan == default)
            {
                timeSpan = TimeSpan.FromSeconds(60);
            }
            var cache = ServiceProvider.GetService<IMemoryCache>();
            var result = cache.Get<T>(name);
            if (result == null)
            {
                result = constructor(ServiceProvider);
                cache.Set(name, result, timeSpan);
            }
            return result;
        }

        public T Get<T>(string key = null, Func<IServiceProvider, T> constructor = null, bool add = true)
        {
            key = key ?? typeof(T).FullName;
            if (dic.TryGetValue(key, out var _res) && _res != null && typeof(T).IsAssignableFrom(_res.GetType()))
            {
                return (T)_res;
            }
            T res = constructor == null ? serviceProvider.GetService<T>() : constructor(serviceProvider);
            if (res != null)
            {
                if (add)
                    dic.AddOrUpdate(key, res, (a, b) => res);
                return res;
            }
            return default;
        }

        internal void SetCannellationToken(CancellationToken token)
        {
            cancellation?.Dispose();
            cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
        }
        internal SPTransmittalItem TransmittalItem { get; set; }
        public SPJobItem Job { get; set; }

    }


    public class QueueContextBase<T> : QueueContextBase where T : QueueContextBase
    {
        private T self => this as T;
        public T WithMaxTrials(int maxTrials)
        {
            MaxTrials = maxTrials;
            return self;
        }
        public T WithAction(Action<T> action)
        {
            action?.Invoke(self);
            return self;
        }
        internal TaskCompletionSource<T> CompletionSource = new TaskCompletionSource<T>();
        public Task<T> CompletionTask => CompletionSource.Task;
        internal Task<T> WithToken(CancellationToken cancellationToken)
        {

            cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            return Task.FromResult(self);
        }
        public T WithServiceProvide(IServiceProvider serviceProvider, bool withoutScope = false)
        {
            if (withoutScope)
            {
                this.serviceProvider = serviceProvider;
            }
            else
            {
                scope = serviceProvider.CreateScope();
            }
            return self;
        }

        public T WithJob(SPJobItem job)
        {
            Job = job;
            return self;
        }
        public T WithTransmittal(SPTransmittalItem transmittal)
        {
            TransmittalItem = transmittal;
            return self;
        }

    }
}


