
using GN.Library.Pipelines.WithBlockingCollection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Pipelines.deprecated
{
    //public enum PipelineEvents
    //{
    //    Step,
    //    Error,
    //    Any

    //}
    //public class PipelineEventArgs
    //{
    //    public PipelineEvents Event { get; }
    //    public IPipelineContext Context { get; }
    //    public Exception LastError { get; }
    //    internal PipelineEventArgs(PipelineEvents ev, IPipelineContext context, Exception lastError)
    //    {
    //        this.Event = ev;
    //        this.Context = context;
    //        this.LastError = lastError;

    //    }
    //}
    //class PipelineEventHandler
    //{
    //    public PipelineEvents Event { get; set; }
    //    public Action<PipelineEventArgs> Handler { get; set; }
    //}
    //public interface IPipelineContext
    //{
    //    IPipelineStep Current { get; }
    //}
    //public interface IPipelineContext<T> : IPipelineContext
    //{
    //    T Helper { get; }
    //}
    //public class PipelineContext<T> : IPipelineContext<T>
    //{
    //    public T Helper { get; set; }
    //    public IPipelineStep Current { get; set; }



    //}
    //public interface IPipelineStep
    //{
    //    string Name { get; }
    //}
    //public class PipelineStep : IPipelineStep
    //{
    //    public string Name { get; }
    //    public Delegate Action { get; }
    //    public PipelineStep(string name, Delegate action)
    //    {
    //        this.Name = name;
    //        this.Action = action;
    //    }

    //}
    
    //public interface IPipeline<TI, TO, TC>
    //{
    //    //TO Start(TI input);
    //    void Start(TI input);
    //    Task StartAsync(TI input);
    //    Task Run(IAsyncEnumerable<TI> items);
    //    IPipeline<TI, T, TC> AddStep<T>(Func<TO, T> func, string name = null);
    //    IPipeline<TI, T, TC> AddStep<T>(Func<TO, Task<T>> func, string name = null);
    //    IPipeline<TI, T, TC> AddStep<T>(Func<TO, IPipelineContext<TC>, T> func, string name = null);
    //    IPipeline<TI, TO, TC> On(PipelineEvents ev, Action<PipelineEventArgs> handler);
    //}
    //class Pipeline<TI, TO, TC> : IPipeline<TI, TO, TC>
    //{
    //    private List<PipelineStep> steps;
    //    private List<PipelineEventHandler> handlers;
    //    private readonly Func<IPipelineContext, TC> factory;

    //    public Pipeline(List<PipelineStep> steps, Func<IPipelineContext, TC> factory, List<PipelineEventHandler> handlers = null)
    //    {
    //        this.steps = steps;
    //        this.factory = factory;
    //        this.steps = this.steps ?? new List<PipelineStep>();
    //        this.handlers = handlers ?? new List<PipelineEventHandler>();
    //    }
    //    private void InvokeHandlers(PipelineEventArgs args)
    //    {
    //        this.handlers.ForEach(x =>
    //        {
    //            if (x != null && (x.Event == args.Event || x.Event == PipelineEvents.Any))
    //            {
    //                try
    //                {
    //                    x.Handler(args);
    //                }
    //                catch { }
    //            }
    //        });

    //    }
    //    private void _start(TI input)
    //    {
    //        object _input = input;
    //        var ctx = new PipelineContext<TC>();
    //        ctx.Helper = this.factory == null ? default(TC) : this.factory.Invoke(ctx);
    //        Pipeline.Create<string>()
    //            .AddStep(x => x.Length);

    //        for (var i = 0; i < this.steps.Count; i++)
    //        {
    //            this.InvokeHandlers(new PipelineEventArgs(PipelineEvents.Step, ctx, null));
    //            var step = this.steps[i];
    //            ctx.Current = step;
    //            if (step.Action is Func<object, object, object> func)
    //            {
    //                try
    //                {
    //                    _input = func(_input, ctx);
    //                }
    //                catch (Exception err)
    //                {
    //                    this.InvokeHandlers(new PipelineEventArgs(PipelineEvents.Error, ctx, err));
    //                }
    //            }
    //        }
    //        //return (TO)_input;
    //    }
    //    private async Task _startAsync(TI input)
    //    {
    //        object _input = input;
    //        var ctx = new PipelineContext<TC>();
    //        ctx.Helper = this.factory == null ? default(TC) : this.factory.Invoke(ctx);
    //        Pipeline.Create<string>()
    //            .AddStep(x => x.Length);

    //        for (var i = 0; i < this.steps.Count; i++)
    //        {
    //            this.InvokeHandlers(new PipelineEventArgs(PipelineEvents.Step, ctx, null));
    //            var step = this.steps[i];
    //            ctx.Current = step;
    //            if (step.Action is Func<object, object, object> func)
    //            {
    //                try
    //                {
    //                    _input = func(_input, ctx);
    //                    if (_input is Task t)
    //                    {
    //                        await t;
    //                        _input = t.GetType().GetProperty("Result")?.GetValue(t);
    //                    }
    //                }
    //                catch (Exception err)
    //                {
    //                    this.InvokeHandlers(new PipelineEventArgs(PipelineEvents.Error, ctx, err));
    //                }
    //            }
    //        }
    //        //return (TO)_input;
    //    }
    //    public void Start(TI input)
    //    {
    //        this._start(input);
    //    }
    //    public Task StartAsync(TI input)
    //    {
    //        return this._startAsync(input);
    //    }
    //    public IPipeline<TI, T, TC> AddStep<T>(Func<TO, T> func, string name = null)
    //    {
    //        Func<object, object, object> func1 = (a, b) =>
    //        {
    //            return (T)func((TO)a);

    //        };
    //        this.steps.Add(new PipelineStep(name, func1));
    //        return new Pipeline<TI, T, TC>(this.steps, this.factory, this.handlers);

    //    }

    //    public IPipeline<TI, T, TC> AddStep<T>(Func<TO, Task<T>> func, string name = null)
    //    {
    //        Func<object, object, object> func1 = (a, b) =>
    //        {
    //            return func((TO)a);

    //        };
    //        this.steps.Add(new PipelineStep(name, func1));
    //        return new Pipeline<TI, T, TC>(this.steps, this.factory, this.handlers);

    //    }

    //    public IPipeline<TI, T, TC> AddStep<T>(Func<TO, IPipelineContext<TC>, T> func, string name = null)
    //    {
    //        Func<object, object, object> func1 = (a, b) =>
    //        {
    //            return (T)func((TO)a, (IPipelineContext<TC>)b);

    //        };
    //        this.steps.Add(new PipelineStep(name, func1));
    //        return new Pipeline<TI, T, TC>(this.steps, this.factory, this.handlers);

    //    }

    //    public IPipeline<TI, TO, TC> On(PipelineEvents ev, Action<PipelineEventArgs> handler)
    //    {
    //        this.handlers.Add(new PipelineEventHandler
    //        {
    //            Event = ev,
    //            Handler = handler

    //        });
    //        return this;
    //    }

    //    public async Task Run(IAsyncEnumerable<TI> items)
    //    {
    //        var f = items.GetAsyncEnumerator();
    //        while (await f.MoveNextAsync())
    //        {
    //            this._start(f.Current);
    //        }
    //    }
    //}

    //public static class Pipeline
    //{
    //    public static IPipeline<T, T, object> Create<T>() => new Pipeline<T, T, object>(null, null);
    //    public static IPipeline<T, T, TC> Create<T, TC>(Func<IPipelineContext, TC> factory) => new Pipeline<T, T, TC>(null, factory);

    //    public static CastingPipelineWithParallelism HH()
    //    {
    //        return new CastingPipelineWithParallelism();
    //    }

    //}
}
