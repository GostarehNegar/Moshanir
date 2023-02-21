using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Functional.Pipelines
{
    public static class PipeExtensions
    {

       
        public static Pipeline<TC, TInput> AddStep<TC, TInput>(this Pipeline<TC, TInput> pipe, Action<TC> action,
            Action<PipelineStepInfo> configure = null) where TC : PipelineContext
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
        public static Pipeline<TC, TInput> AddAction<TC, TInput>(this Pipeline<TC, TInput> pipe, Func<TC, TC> action) where TC : PipelineContext
        {
            return pipe.DoAddStep(async x =>
            {
                if (action != null)
                {
                    return action(x);
                }
                await Task.CompletedTask;
                return x;
            }, cfg => { cfg.Name = action.Method.Name; });
        }
        public static Pipeline<TC, TInput> StopIf<TC, TInput>(this Pipeline<TC, TInput> pipe, Func<TC, bool> action) where TC : PipelineContext
        {
            return pipe.DoAddStep(async x =>
            {
                if (action != null)
                {
                    if (action(x))
                    {
                        pipe.Stop();
                    }
                }
                await Task.CompletedTask;
                return x;
            }, cfg => { cfg.Name = action.Method.Name; });

        }
        public static Pipeline<TC, TInput> AddStep<TC, TInput>(this Pipeline<TC, TInput> pipe, Func<TC, TC> action) where TC : PipelineContext
        {
            return pipe.DoAddStep(async x =>
            {
                if (action != null)
                {
                    return action(x);
                }
                await Task.CompletedTask;
                return x;
            }, cfg => { cfg.Name = action.Method.Name; });
        }
        public static Pipeline<TC, TInput> AddStep<TC, TInput>(this Pipeline<TC, TInput> pipe, Func<TC, Task<TC>> action) where TC : PipelineContext
        {
            return pipe.DoAddStep(async x =>
            {
                if (action != null)
                {
                    return await action(x);
                }
                return x;
            }, cfg => { cfg.Name = action.Method.Name; });
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


        public static Pipeline<TC, TInput> AddStep<TC, TInput>(
            this Pipeline<TC, TInput> pipe, Func<IPipelineContext,
                IPipelineContext> action, Action<PipelineStepInfo> configure = null) where TC : PipelineContext
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
            this Pipeline<TC, TInput> pipe, Func<IPipelineContext, Task<IPipelineContext>> action,
            Action<PipelineStepInfo> configureStep = null) where TC : PipelineContext
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

        //public static Pipeline<IFunctionalContext<T2>, TInput> Cast<TC, TInput, T2>(
        //    this Pipeline<TC, TInput> pipe, Func<TC, Task<T2>> action, Action<StepInfo> configureStep = null) where TC : FunctionalContext
        //{
        //    if (typeof(T2) == GetStateType<TC>())
        //    {
        //        var result = pipe.DoAddStep(async x =>
        //        {

        //            var res = await action(x);
        //            return x.Cast<TC>(res);
        //        }, step =>
        //        {
        //            step.Name = action.Method.Name;
        //            configureStep?.Invoke(step);
        //        });
        //        return result as Pipeline<IFunctionalContext<T2>, TInput>;
        //    }
        //    else
        //    {
        //        return pipe.DoCast<IFunctionalContext<T2>>(async x =>
        //        {
        //            var res = await action(x);
        //            return x.Cast<IFunctionalContext<T2>>(res);
        //            //return new ValueTask<IFunctionalContext<T2>>(x.Cast<IFunctionalContext<T2>>(res));
        //        }, step =>
        //        {
        //            step.Name = action.Method.Name;
        //            configureStep?.Invoke(step);
        //        });
        //    }
        //}



        //public static Pipeline<IFunctionalContext<T2>, TInput> Cast<TC, TInput, T2>(
        //    this Pipeline<TC, TInput> pipe, Func<TC, T2> action, Action<StepInfo> configureStep = null)
        //    where TC : IFunctionalContext
        //{
        //    if (typeof(T2) == GetStateType<TC>())
        //    {
        //        var result = pipe.DoAddStep(x =>
        //        {

        //            var res = action(x);
        //            return new ValueTask<TC>(x.Cast<TC>(res));
        //        }, step =>
        //        {
        //            step.Name = action.Method.Name;
        //            configureStep?.Invoke(step);
        //        });
        //        return result as Pipeline<IFunctionalContext<T2>, TInput>;
        //    }
        //    else
        //    {
        //        return pipe.DoCast<IFunctionalContext<T2>>(async x =>
        //        {
        //            var res = action(x);
        //            return x.Cast<IFunctionalContext<T2>>(res);
        //        }, step =>
        //        {
        //            step.Name = action.Method.Name;
        //            configureStep?.Invoke(step);
        //        });
        //    }
        //}

        public static Pipeline<TN, TInput> Cast<TC, TInput, TN>(
            this Pipeline<TC, TInput> pipe, Func<TC, TN> step, Action<PipelineStepInfo> configure = null)
            where TC : PipelineContext
            where TN : PipelineContext
        {
            return pipe.DoCast(async x => step(x), s =>
             {
                 s.Name = step.Method.Name;
                 configure?.Invoke(s);
             });

        }
        public static Pipeline<TC, TInput> AddStep<TC, TInput>(
            this Pipeline<TC, TInput> pipe, Func<TC, ValueTask<TC>> step, Action<PipelineStepInfo> configureStep = null)
            where TC : PipelineContext
        {
            return pipe.DoAddStep(step, s =>
            {
                s.Name = step.Method.Name;
                configureStep?.Invoke(s);
            });
        }

        public static PipelineStepInfo StepInfo(this IPipelineContext context, PipelineStepInfo val = null)
        {
            return val == null
                ? context.GetValue<PipelineStepInfo>("$stepinfo", s => null, overWrite: false)
                : context.GetValue<PipelineStepInfo>("$stepinfo", s => val, overWrite: true);
        }

        public static Pipeline<TState> CreatePipe<TState>(this IServiceProvider serviceProvider, string name)
        {
            return new Pipeline<TState>(name, serviceProvider);
        }
        public static PipelineContext GetConcreteFunctionalContext(this IPipelineContext This)
        {
            if (This is PipelineContext r)
            {
                return r;
            }
            throw new Exception(
                $"Invalid Context");
        }
    }
}
