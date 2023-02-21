using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Pipelines.deprecated2
{
    public static class PipelineExtensions
    {
        public static Pipeline<TC, TInput> AddStep<TC, TInput>(this Pipeline<TC, TInput> pipe, Action<TC> action,
            Action<StepInfo> configure = null) where TC : PipelineContext
        {
            return pipe.DoAddStep(x =>
            {
                action?.Invoke(x);
                return new ValueTask<TC>(x);
            }, s =>
            {
                s.Name = action.Method.Name;
                configure?.Invoke(s);
            });
        }
        public static Pipeline<TC, TInput> AddStep<TC, TInput>(this Pipeline<TC, TInput> pipe, Func<TC, Task> action) where TC : PipelineContext
        {
            return pipe.DoAddStep(async x =>
            {
                if (action != null)
                {
                    await action(x);
                }
                return x;
            }, cfg => { cfg.Name = action.Method.Name; });
        }
        internal static Type GetStateType<TC>() where TC : PipelineContext
        {
            return typeof(TC).GetGenericArguments().Length == 1 ? typeof(TC).GetGenericArguments()[0] : typeof(object);
        }
        public static ValueTask<PipelineContext<T>> GetValueTask<T>(T val)
        {
            return new ValueTask<PipelineContext<T>>(new PipelineContext<T>(val));
        }

        public static Pipeline<TC, TInput> AddStep<TC, TInput>(
            this Pipeline<TC, TInput> pipe, Func<PipelineContext, PipelineContext> action, Action<StepInfo> configure = null) where TC : PipelineContext
        {
            return pipe.DoAddStep(async x =>
            {
                if (action != null)
                {
                    return action.Invoke(x).Cast<TC>();
                }
                return x;
            }, configure);
        }
        public static Pipeline<TC, TInput> AddStep<TC, TInput>(
            this Pipeline<TC, TInput> pipe, Func<PipelineContext, Task<PipelineContext>> action,
            Action<StepInfo> configureStep = null) where TC : PipelineContext
        {
            return pipe.DoAddStep(async x =>
            {
                if (action != null)
                {
                    return (await action.Invoke(x)).Cast<TC>();
                }
                return x;
            }, step =>
            {
                step.Name = action.Method.Name;
                configureStep?.Invoke(step);
            });
        }

        public static Pipeline<PipelineContext<T2>, TInput> Cast<TC, TInput, T2>(
            this Pipeline<TC, TInput> pipe, Func<TC, Task<T2>> action, Action<StepInfo> configureStep = null) where TC : PipelineContext
        {
            if (typeof(T2) == GetStateType<TC>())
            {
                var result = pipe.DoAddStep(async x =>
                {

                    var res = await action(x);
                    return x.Cast<TC>(res);
                }, step =>
                {
                    step.Name = action.Method.Name;
                    configureStep?.Invoke(step);
                });
                return result as Pipeline<PipelineContext<T2>, TInput>;
            }
            else
            {
                return pipe.DoCast<PipelineContext<T2>>(async x =>
                {
                    var res = await action(x);
                    return x.Cast<PipelineContext<T2>>(res);
                    //return new ValueTask<PipelineContext<T2>>(x.Cast<PipelineContext<T2>>(res));
                }, step =>
                {
                    step.Name = action.Method.Name;
                    configureStep?.Invoke(step);
                });
            }
        }
        


        public static Pipeline<PipelineContext<T2>, TInput> Cast<TC, TInput, T2>(
            this Pipeline<TC, TInput> pipe, Func<TC, T2> action, Action<StepInfo> configureStep = null)
            where TC : PipelineContext
        {
            if (typeof(T2) == GetStateType<TC>())
            {
                var result = pipe.DoAddStep(x =>
                 {

                     var res = action(x);
                     return new ValueTask<TC>(x.Cast<TC>(res));
                 }, step =>
                 {
                     step.Name = action.Method.Name;
                     configureStep?.Invoke(step);
                 });
                return result as Pipeline<PipelineContext<T2>, TInput>;
            }
            else
            {
                return pipe.DoCast<PipelineContext<T2>>(async x =>
                {
                    var res = action(x);
                    return x.Cast<PipelineContext<T2>>(res);
                }, step =>
                {
                    step.Name = action.Method.Name;
                    configureStep?.Invoke(step);
                });
            }
        }

        internal static Pipeline<TN, TInput> Cast<TC, TInput, TN>(
            this Pipeline<TC, TInput> pipe, Func<TC, ValueTask<TN>> step, Action<StepInfo> configure = null)
            where TC : PipelineContext
            where TN : PipelineContext
        {
            return pipe.DoCast(step, s =>
            {
                s.Name = step.Method.Name;
                configure?.Invoke(s);
            });

        }
        public static Pipeline<TC, TInput> AddStep<TC, TInput>(
            this Pipeline<TC, TInput> pipe, Func<TC, ValueTask<TC>> step, Action<StepInfo> configureStep = null)
            where TC : PipelineContext
        {
            return pipe.DoAddStep(step, s =>
            {
                s.Name = step.Method.Name;
                configureStep?.Invoke(s);
            });
        }

        public static StepInfo StepInfo(this PipelineContext context, StepInfo val = null)
        {
            return val == null
                ? context.Get<StepInfo>(s => null, "$stepinfo", overWrite: false)
                : context.Get<StepInfo>(s => val, "$stepinfo", overWrite: true);
        }

        public static Pipeline<TState> CreatePipe<TState>(this IServiceProvider serviceProvider, string name)
        {
            return new Pipeline<TState>(name, serviceProvider);
        }
    }
}
