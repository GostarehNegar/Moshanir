using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GN.Library.Functional.Pipelines
{
    public class PipelineContext : IPipelineContext
    {
        protected object state;
        protected Type stateType;
        public Object State => state;
        public event EventHandler<PipeEventArgs> On;
        protected IServiceScope scope;
        private bool disposedValue;
        protected ConcurrentDictionary<string, object> values = new ConcurrentDictionary<string, object>();
        public PipelineContext()
        {

        }
        public PipelineContext(object state, IServiceScope scope)
        {
            this.scope = scope;
            this.state = state;
        }

        public IServiceProvider ServiceProvider => this.scope?.ServiceProvider;

        #region Disposable Pattern
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    scope?.Dispose();
                }
                this.scope = null;
                disposedValue = true;
            }
        }
        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~FPContext()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public virtual void SetState(object state)
        {
            this.Validate(state);
            this.state = state;
        }
        public virtual T Cast<T>(bool force = true) where T : IPipelineContext
        {
            return DoCast<T>(this.state, false, false);
        }
        public virtual T Cast<T>(object state) where T :  IPipelineContext
        {
            var result = DoCast<T>(state, true);
            return result;
        }
        private T DoCast<T>(object state, bool hasstate, bool force = true) where T :IPipelineContext
        {
            var This = this as PipelineContext;
            if (This==null)
            {
                throw new Exception($"Invalid Context");
            }
            
            if (!force && this is T r)
            {
                if (hasstate)
                {
                    This.SetState(state);
                }
                return r;
            }
            T result = default(T);
            if (this.ServiceProvider != null)
            {
                result = ActivatorUtilities.CreateInstance<T>(this.ServiceProvider);
            }
            else
            {
                result = Activator.CreateInstance<T>();
            }
            if (result == null)
            {
                throw new Exception(
                    $"Failed to create context. Type:{typeof(T).FullName}");
            }
            var _state = hasstate ? state : this.state;
            result?.Init(_state, this.scope, this.values);
            (result as PipelineContext).On = this.On;
            return result;
        }
        public virtual IPipelineContext Init(object state, IServiceScope scope, IDictionary<string, object> values)
        {
            this.scope = scope;
            this.values = new ConcurrentDictionary<string, object>(values ?? new Dictionary<string, object>());
            this.SetState(state);
            return this;
        }
        public TVal GetValue<TVal>(string key,
                              Func<IServiceProvider, TVal> constructor = null,
                              bool cache = true,
                              bool overWrite = false,
                              bool dontUseServiceProvider = false,
                              TimeSpan expires = default)
        {
            TVal result = default(TVal);
            key = key ?? typeof(TVal).FullName;

            TVal construct()
            {
                return constructor == null
                   ? this.ServiceProvider == null || dontUseServiceProvider ? default : this.ServiceProvider.GetServiceEx<TVal>()
                   : constructor(this.ServiceProvider);
            }
            if (!cache)
            {

                return construct();
            }
            else if (this.values.TryGetValue(key, out var res) && res != null && typeof(TVal).IsAssignableFrom(res.GetType()))
            {
                if (!overWrite)
                {
                    return (TVal)res;
                }
            }
            else
            {
                result = construct();
                if (result != null)
                {
                    this.values.AddOrUpdate(key, result, (a, b) => result);
                }
                return result;
            }
            return result;

        }

        public void RemoveValue<TVal>(string key = null)
        {
            key = key ?? typeof(TVal).FullName;
            this.values.TryRemove(key, out _);
        }

        public override string ToString()
        {
            return state?.ToString();
        }

        public IPipeContext<TO> Clone<TO>(object state)
        {
            if (this is PipeContext<TO> s)
            {
                s.SetState(state);
                return s;
            }
            return new PipeContext<TO>()
                .Init(state, this.scope, this.values) as IPipeContext<TO>;
        }

        public virtual void Validate(object state)
        {

        }

        public void Invoke(PipeEventArgs e)
        {
            this.On?.Invoke(this, e);
        }

    }

    public class PipeContext<T> : PipelineContext, IPipeContext<T>
    {
        internal static bool IsNullable(Type type)
        {
            return type == null || !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        public new T State => (T)state;
        public PipeContext() : base(null, null)
        {

        }
        public PipeContext(T state, IServiceScope scope) : base(state, scope)
        {
            this.stateType = typeof(T);
        }
        
        public IPipeContext<T> Init(T state, IServiceScope scope, IDictionary<string, object> values)
        {
            base.Init(state, scope, values);
            return this;
        }

        public IPipeContext<T> Clone(T state)
        {
            return base.Clone<T>(state);
            return new PipeContext<T>().Init(state, this.scope, this.values);
        }

        public override void Validate(object state)
        {
            if ((state != null && !typeof(T).IsAssignableFrom(state.GetType())) ||
               (state == null && !IsNullable(typeof(T))))
            {
                throw new ArgumentException(
                    $"Invalid State. State type should be:{typeof(T).FullName}.");
            }
        }
    }
}
