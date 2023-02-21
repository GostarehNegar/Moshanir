using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace GN.Library.Functional.Pipelines
{
    public interface IPipelineContext : IDisposable
    {
        event EventHandler<PipeEventArgs> On;
        IServiceProvider ServiceProvider { get; }
        IPipelineContext Init(object state, IServiceScope scope, IDictionary<string, object> values = null);
        TVal GetValue<TVal>(string key, Func<IServiceProvider, TVal> constructor = null,
                              bool cache = true,
                              bool overWrite = false,
                              bool dontUseServiceProvider = false,
                              TimeSpan expires = default);
        void RemoveValue<TVal>(string key = null);
        IPipeContext<TO> Clone<TO>(object state);
        Object State { get; }

        T Cast<T>(object state) where T :  IPipelineContext;
        T Cast<T>(bool force = true) where T :  IPipelineContext;

        void SetState(object state);


    }
    public interface IPipeContext<T> : IPipelineContext
    {
        new T State { get; }
        IPipeContext<T> Init(T state, IServiceScope scope, IDictionary<string, object> values = null);
        IPipeContext<T> Clone(T state);

        
    }
}
