using GN.Library.Functional.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GN.Library.Functional
{
    public static class Functions
    {
        internal static bool IsNullable(Type type)
        {
            return type == null || !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }
        
        public static T CreateContext<T>(this IServiceProvider This, object state) where T : class, IPipelineContext
        {
            return ActivatorUtilities
                .CreateInstance<T>(This)
                .Init(state, This.CreateScope(), null) as T;
        }
        public static IPipeContext<T> CreateContext<T>(this IServiceProvider This, T state)
        {
            return new PipeContext<T>().Init(state, This.CreateScope(), null);
        }
    }
}
