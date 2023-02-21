using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace GN.Library.Functional
{

    public interface IServiceScopeEx<out T> : IServiceScope
    {
        IDictionary<string, object> Parameters { get; }
        T Context { get; }
    }
    public class ServiceScopeEx<T> : IServiceScopeEx<T>
    {
        private ConcurrentDictionary<string, object> parameters = new ConcurrentDictionary<string, object>();
        private IServiceScope scope;
        public IDictionary<string, object> Parameters => parameters;
        public T Context { get; private set; }
        public IServiceProvider ServiceProvider => this.scope.ServiceProvider;

        public ServiceScopeEx(IServiceProvider serviceProvider, IDictionary<string, object> parameters= null)
        {
            this.scope = serviceProvider.CreateScope();
            this.parameters = new ConcurrentDictionary<string, object>(parameters ?? new Dictionary<string, object>());
        }
        public void Dispose()
        {
            this.scope?.Dispose();

        }
    }

    //    public interface IServiceContext : IDisposable
    //    {
    //        IServiceProvider ServiceProvider { get; }
    //        IDictionary<string, object> ValueProvider { get; }
    //    }
    //    public class ServiceContext : IServiceContext
    //    {
    //        protected readonly IServiceScope scope;
    //        protected ConcurrentDictionary<string, object> values;
    //        public IServiceProvider ServiceProvider => this.scope.ServiceProvider;
    //        public IDictionary<string, object> ValueProvider => values;
    //        public ServiceContext(IServiceScope scope, IDictionary<string, object> values)
    //        {
    //            this.scope = scope;
    //            this.values = new ConcurrentDictionary<string, object>(values ?? new Dictionary<string, object>());
    //        }
    //        public T GetValue<T>(string key, Func<ServiceContext, T> constructor)
    //        {
    //            key = key ?? typeof(T).FullName;
    //            if (this.values.TryGetValue(key, out var _res)
    //                && _res != null
    //                && typeof(T).IsAssignableFrom(_res.GetType()))
    //            {
    //                return (T)(object)_res;
    //            }
    //            if (constructor != null)
    //            {
    //                var val = constructor(this);
    //                this.values.AddOrUpdate(key, val, (a, b) => val);
    //                return val;
    //            }
    //            return default;
    //        }
    //        public void SetValue<T>(string key, T value)
    //        {
    //            key = key ?? typeof(T).FullName;
    //            this.values.AddOrUpdate(key, value, (_x, _y) => value);
    //        }
    //        public void RemoveValue(string key)
    //        {
    //            this.values.TryRemove(key, out var _);
    //        }
    //        public void Dispose()
    //        {
    //            this.scope?.Dispose();
    //        }
    //    }
    //    public class ContextWithState<T>:IDisposable
    //    {
    //        public T State { get; }
    //        public IServiceContext Context { get; }
    //        public ContextWithState(T state, IServiceContext context)
    //        {
    //            State = state;
    //            Context = context;
    //        }

    //        public void Dispose()
    //        {
    //            this.Context?.Dispose();
    //        }
    //    }
    //    class MyContext<T> : ContextWithState<T>
    //    {
    //        public MyContext(T state, ServiceContext context) : base(state, context)
    //        {
    //        }
    //    }
}
