using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GN.Library.Pipelines.deprecated2
{
    public enum PipelineEvents
    {
        Completed,
        Error,
        Step,

    }
    public class PipelineEventArgs
    {
        public PipelineEvents Event { get; }
        public PipelineContext Context { get; }
        public StepInfo Step { get; }
        public Exception LastError { get; }
        internal PipelineEventArgs(PipelineEvents ev, PipelineContext context, Exception lastError, StepInfo step=null)
        {
            this.Event = ev;
            this.Context = context;
            this.LastError = lastError;
            this.Step = step;
        }
        public override string ToString()
        {
            return $"Event: '{Event}' in step: '{this.Step?.Name}'";
        }
        internal static PipelineEventArgs Completed(PipelineContext context)
        {
            return new PipelineEventArgs(PipelineEvents.Completed, context, null);
        }
        internal static PipelineEventArgs StepStart(PipelineContext context, StepInfo step)
        {
            return new PipelineEventArgs(PipelineEvents.Step, context, null,step);
        }
        internal static PipelineEventArgs Error(PipelineContext context, StepInfo step, Exception exception)
        {
            return new PipelineEventArgs(PipelineEvents.Error, context, exception, step);
        }
    }
    public class PipelineContext : IDisposable
    {
        public event EventHandler<PipelineEventArgs> On;
        protected object state;
        protected ConcurrentDictionary<string, object> values;
        protected IServiceScope scope;
        public IServiceProvider ServiceProvider => this.scope?.ServiceProvider;
        public object State => this.state;
        public PipelineContext()
        {

        }

        public PipelineContext(object state, IServiceScope scope = null, IDictionary<string, object> values = null)
        {
            this.state = state;
            this.scope = scope;
            this.values = new ConcurrentDictionary<string, object>(values ?? new Dictionary<string, object>());
            //this.Initialize(state, scope, values);
        }

        public virtual T Cast<T>(object state) where T : PipelineContext
        {
            var result = DoCast<T>(state, true);
            return result;
        }
        private T DoCast<T>(object state, bool hasstate, bool force = true) where T : PipelineContext
        {
            if (!force && this is T r)
            {
                return r;
            }
            T result = null;
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
            result?.Initialize(_state, this.scope, this.values);
            result.values = this.values;
            result.On = this.On;
            return result;
        }
        public virtual T Cast<T>(bool force = true) where T : PipelineContext
        {
            return DoCast<T>(null, false, force);
            if (!force && this is T r)
            {
                return r;
            }
            T result = null;
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
            result?.Initialize(this.state, this.scope, this.values);
            return result;
        }

        public void Initialize(object state, IServiceScope scope = null, IDictionary<string, object> values = null)
        {
            this.state = state;
            this.scope = scope;
            this.values = new ConcurrentDictionary<string, object>(values ?? new Dictionary<string, object>());
            this.Validate();
        }
        public virtual bool Validate(bool throwIfNotValiad = true)
        {
            return true;
        }

        public override string ToString()
        {
            return this.state?.ToString();
        }

        public TVal Get<TVal>(Func<IServiceProvider, TVal> constructor, string key = null, bool cache = true, bool overWrite = false, TimeSpan expires = default)
        {
            TVal result = default(TVal);
            key = key ?? typeof(TVal).FullName;

            TVal construct()
            {
                return constructor == null
                   ? this.ServiceProvider == null ? default : this.ServiceProvider.GetServiceEx<TVal>()
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

        public void Dispose()
        {
            this.scope?.Dispose();
        }
        public void Invoke(PipelineEventArgs e)
        {
            this.On?.Invoke(this, e);
        }
    }

    public class PipelineContext<TState> : PipelineContext
    {
        public new TState State => (TState)this.state;

        public PipelineContext()
        {

        }
        public PipelineContext(TState state, IServiceScope scope = null, IDictionary<string, object> values = null)
            : base(state, scope, values)
        {
        }
        new public void Initialize(object state, IServiceScope scope = null, IDictionary<string, object> values = null)
        {
            this.Initialize((TState)state, scope, values);
        }
        public void Initialize(TState state, IServiceScope scope = null, IDictionary<string, object> values = null)
        {
            base.Initialize(state, scope, values);
        }
        public override bool Validate(bool throwIfNotValiad = true)
        {
            if (this.state != null && !typeof(TState).IsAssignableFrom(this.state.GetType()))
            {
                if (throwIfNotValiad)
                {
                    throw new Exception(
                        $"Invalid State. State should be {typeof(TState).FullName}");
                }
                return false;
            }
            return base.Validate(throwIfNotValiad);

        }

    }
}
