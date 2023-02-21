using GN.Library.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Pipelines.deprecated
{


    public class PipeContext_dep : IDisposable
    {
        protected IServiceScope scope;
        protected ConcurrentDictionary<string, object> values = new ConcurrentDictionary<string, object>();
        public IServiceProvider ServiceProvider => scope.ServiceProvider;
        protected object state;
        public object State => state;
        public string Name { get; private set; }
        public ILogger Logger { get; private set; }

        public PipeContext_dep(string name, IServiceScope scope, object state)
        {
            this.scope = scope;
            this.state = state;
            this.Name = string.IsNullOrWhiteSpace(name) ? "Pipe" : name;
            this.Logger = this.ServiceProvider?.GetServiceEx<ILoggerFactory>()?.CreateLogger(this.Name);
            //this.Logger.in

        }
        public void Dispose()
        {
            this.scope?.Dispose();
            this.scope = null;
        }

        public PipeContext_dep<TO> Cast<TO>()
        {
            var result = new PipeContext_dep<TO>(this.Name, this.scope, (TO)this.State);
            result.values = this.values;
            //this.Dispose();
            return result;
        }
        internal PipeContext_dep<T> As<T>()
        {
            if (this as PipeContext_dep<T> == null)
            {
                throw new Exception($"Invalid Cast!!!");
            }
            return this as PipeContext_dep<T>;
        }
        public override string ToString()
        {
            return this.State.ToString();
        }
    }
    public class PipeContext_dep<T> : PipeContext_dep
    {
        public PipeContext_dep(string name, IServiceScope scope, T state = default(T)) : base(name, scope, state)
        {
            this.state = state;
        }
        public new T State => (T)state;
        public ValueTask<T> AsStateTask => new ValueTask<T>(this.State);
        internal PipeContext_dep<T> SetState(T state)
        {
            base.state = state;
            //this.State = state;
            return this;
        }
        internal PipeContext_dep<TO> _Cast<TO>(TO state)
        {
            var result = new PipeContext_dep<TO>(this.Name, this.scope, state);
            result.values = this.values;
            //this.Dispose();
            return result;
        }
    }
    public class Pipe_dep<TState, TInput>
    {

        private Func<PipeContext_dep, ValueTask<PipeContext_dep>> pipe;
        private IServiceProvider serviceProvider;
        internal Pipe_dep(
            Func<PipeContext_dep, ValueTask<PipeContext_dep>> pipe = null,
            IServiceProvider provider = null)
        {
            this.pipe = pipe ?? (ctx => new ValueTask<PipeContext_dep>(ctx));
            this.serviceProvider = provider;
        }

        private Pipe_dep<TState, TInput> DoAddStep(Func<PipeContext_dep<TState>, ValueTask<TState>> f)
        {
            var pipe = this.pipe;
            this.pipe = async input =>
            {
                try
                {
                    var ctx = (await pipe(input)).As<TState>();
                    //var ns = await f(ctx);
                    return ctx.SetState(await f(ctx));
                }
                catch (Exception err)
                {
                    throw;
                }
                return input;
            };
            return this;
        }
        public Pipe_dep<TO, TInput> DoCast<TO>(Func<PipeContext_dep<TState>, ValueTask<TO>> f)
        {
            var p = this.pipe;
            this.pipe = async input =>
            {
                //var ctx = input as PipelineContextEx<TState>;
                var hhh = await (p(input));
                var ctx = hhh as PipeContext_dep<TState>;
                var g = await f(ctx);
                return ctx._Cast<TO>(g);

            };
            var res = new Pipe_dep<TO, TInput>(pipe, this.serviceProvider);
            return res;
        }
        public Pipe_dep<TO, TInput> AddStep<TO>(Func<PipeContext_dep<TState>, TO> f)
        {
            return this.DoCast<TO>(ctx => new ValueTask<TO>(f(ctx)));
        }
        public Pipe_dep<TO, TInput> AddStep<TO>(Func<PipeContext_dep<TState>, Task<TO>> f)
        {
            return this.DoCast<TO>(ctx => new ValueTask<TO>(f(ctx)));
        }
        public Pipe_dep<TO, TInput> AddStep<TO>(Func<PipeContext_dep<TState>, ValueTask<TO>> f)
        {
            return this.DoCast<TO>(f);
        }
        public Pipe_dep<TState, TInput> AddStep(Func<PipeContext_dep, Task> func)
        {
            this.DoAddStep(async ctx =>
            {
                await func(ctx);
                return ctx.State;
            });
            return this;
        }
        public Pipe_dep<TState, TInput> AddStep(Func<PipeContext_dep<TState>, Task<TState>> func)
        {
            return this.DoAddStep(ctx => new ValueTask<TState>(func(ctx)));
        }
        public Pipe_dep<TState, TInput> AddStep(Action<PipeContext_dep<TState>> f)
        {
            this.AddStep(ctx => { f?.Invoke(ctx); return ctx.AsStateTask; });
            return this;
        }
        public Pipe_dep<TState, TInput> AddStep(Func<PipeContext_dep<TState>, TState> f)
        {
            this.AddStep(ctx => new ValueTask<TState>(f.Invoke(ctx)));
            return this;
        }
        public Pipe_dep<TState, TInput> AddStep(Func<PipeContext_dep<TState>, ValueTask> f)
        {
            return this;
        }
        public Pipe_dep<TState, TInput> AddStep(Func<PipeContext_dep<TState>, ValueTask<TState>> f)
        {
            return DoAddStep(f);
        }

        public async ValueTask<PipeContext_dep<TState>> RunEx(TInput input, string name = null)
        {
            var ctx = new PipeContext_dep<TInput>(name, this.serviceProvider.CreateScope(), input);
            {
                var result = (await this.pipe(ctx)) as PipeContext_dep<TState>;
                return result;
            }
        }
        public async ValueTask<TState> Run(TInput input)
        {
            return (await RunEx(input)).State;
        }
    }
    public class Pipe_dep<TC> : Pipe_dep<TC, TC>
    {
        private Pipe_dep(IServiceProvider provider) : base(null, provider: provider)
        {
        }

        public static Pipe_dep<TC> Create(IServiceProvider provider)
        {

            return new Pipe_dep<TC>(provider);
        }
        public static Pipe_dep<TC> Create(Action<IServiceCollection> configure)
        {
            var services = new ServiceCollection();
            var host = new HostBuilder()
            .UseDefaultServiceProvider(s => s.ValidateScopes = false)
            .ConfigureLogging(l => { }) //l.AddConsole())
            .ConfigureServices((c, s) =>
            {
                configure?.Invoke(s);
            }).Build();



            return new Pipe_dep<TC>(host.Services);
        }
    }

    
}
