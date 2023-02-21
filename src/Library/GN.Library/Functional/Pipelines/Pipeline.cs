using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace GN.Library.Functional.Pipelines
{
    public class Pipeline<TC, TInput> where TC : PipelineContext
    {
        const bool ListMode = true;
        private Func<IPipelineContext, object, ValueTask<IPipelineContext>> pipe;
        private List<Func<IPipelineContext, ValueTask<IPipelineContext>>> steps;
        private Func<IPipelineContext, Exception, ValueTask<IPipelineContext>> exceptionHandler;
        private Func<IPipelineContext, ValueTask> finalBlock;
        private readonly IServiceProvider serviceProvider;
        private CancellationTokenSource cancellation;
        public string Name { get; protected set; }
        public void Stop()
        {
            this.cancellation?.Cancel();
        }
        public Pipeline(
            string name,
            IServiceProvider serviceProvider = null,
            Func<IPipelineContext, object, ValueTask<IPipelineContext>> pipe = null,
            List<Func<IPipelineContext, ValueTask<IPipelineContext>>> steps = null,
            Func<IPipelineContext, Exception, ValueTask<IPipelineContext>> exceptionHandler = null,
            Func<IPipelineContext, ValueTask> finalBlock = null)
        {
            var stateType = typeof(TC).GetGenericArguments().Length == 1 ? typeof(TC).GetGenericArguments()[0] : typeof(object);
            this.Name = name ?? $"Pipe<{stateType.Name}";
            this.pipe = pipe ?? ((ctx, h) => new ValueTask<IPipelineContext>(ctx));
            this.steps = steps ?? new List<Func<IPipelineContext, ValueTask<IPipelineContext>>>();
            this.serviceProvider = serviceProvider ?? new ServiceCollection().BuildServiceProvider();
            this.exceptionHandler = exceptionHandler;
            this.finalBlock = finalBlock;
        }
        internal Pipeline<TC, TInput> DoAddStep(Func<TC, ValueTask<TC>> step, Action<PipelineStepInfo> configure)
        {
            return ListMode ? DoAddStepWithList(step, configure) : DoAddStepWithPipe(step, configure);
        }
        internal Pipeline<TN, TInput> DoCast<TN>(Func<TC, ValueTask<TN>> step, Action<PipelineStepInfo> configure) where TN : PipelineContext
        {
            return ListMode ? DoCastWithList(step, configure) : DoCastWithPipe<TN>(step, configure);
        }
        #region pipe
        private Pipeline<TC, TInput> DoAddStepWithPipe(Func<TC, ValueTask<TC>> step, Action<PipelineStepInfo> configure)
        {
            var pipe = this.pipe;
            var stepInfo = new PipelineStepInfo
            {
                Name = step.Method.Name
            };
            configure?.Invoke(stepInfo);

            this.pipe = async (input, o) =>
            {
                try
                {
                    var res = (await pipe(input, o)).Cast<TC>();
                    //res.StepInfo(stepInfo);
                    res.Invoke(PipeEventArgs.StepStart(res, stepInfo));
                    return await step(res);
                }
                catch (Exception err)
                {
                    input.GetConcreteFunctionalContext().Invoke(PipeEventArgs.Error(input, stepInfo, err));
                    input.StepInfo(stepInfo);
                    if (this.exceptionHandler != null)
                    {
                        return await this.exceptionHandler(input.Cast<TC>(), err);
                    }
                    throw;
                }
            };
            return this;
        }
        private Pipeline<TN, TInput> DoCastWithPipe<TN>(Func<TC, ValueTask<TN>> step, Action<PipelineStepInfo> configure) where TN : PipelineContext
        {
            var pipe = this.pipe;
            var stepInfo = new PipelineStepInfo
            {
                Name = step.Method.Name
            };
            configure?.Invoke(stepInfo);
            this.pipe = async (input, o) =>
            {
                try
                {
                    //return await step((await pipe(input, o)).Cast<TC>());
                    var res = (await pipe(input, o)).Cast<TC>();
                    //res.StepInfo(stepInfo);
                    res.Invoke(PipeEventArgs.StepStart(res, stepInfo));
                    return await step(res);
                }
                catch (Exception err)
                {
                    input.GetConcreteFunctionalContext()?.Invoke(PipeEventArgs.Error(input, stepInfo, err));
                    input.StepInfo(stepInfo);
                    if (o != null && o is Pipeline<TN, TInput> aa && aa.exceptionHandler != null)
                    {
                        return await aa.exceptionHandler(input.Cast<TC>(), err);
                    }
                    if (this.exceptionHandler != null)
                    {
                        return await this.exceptionHandler(input.Cast<TC>(), err);
                    }
                    throw;
                }
            };
            return new Pipeline<TN, TInput>(this.Name, this.serviceProvider, this.pipe, this.steps, this.exceptionHandler, this.finalBlock);
        }
        public async ValueTask<TC> RunWithPipe(IPipeContext<TInput> ctx)
        {
            TC result = null;
            try
            {
                result = (await this.pipe(ctx, this)).Cast<TC>();
                result.Invoke(PipeEventArgs.Completed(result));

                return result;

            }
            finally
            {
                if (this.finalBlock != null)
                {
                    if (result is TC r)
                    {
                        await this.finalBlock(r);
                    }
                }
            }
        }

        #endregion

        #region List
        private Pipeline<TC, TInput> DoAddStepWithList(Func<TC, ValueTask<TC>> step, Action<PipelineStepInfo> configure)
        {
            var pipe = this.pipe;
            var stepInfo = new PipelineStepInfo
            {
                Name = step.Method.Name
            };
            configure?.Invoke(stepInfo);
            this.steps.Add(async (c) =>
            {
                var ret = await step(c.Cast<TC>());
                return ret;
            });
            return this;
        }
        private Pipeline<TN, TInput> DoCastWithList<TN>(Func<TC, ValueTask<TN>> step, Action<PipelineStepInfo> configure) where TN : PipelineContext
        {
            var stepInfo = new PipelineStepInfo
            {
                Name = step.Method.Name
            };
            configure?.Invoke(stepInfo);
            this.steps.Add(async (c) =>
            {
                var ret = await step(c.Cast<TC>());
                return ret;
            });
            return new Pipeline<TN, TInput>(this.Name, this.serviceProvider, this.pipe, this.steps, this.exceptionHandler, this.finalBlock);
        }

        public async ValueTask<TC> RunWithList(IPipeContext<TInput> ctx, CancellationToken cancellationToken)
        {
            TC result = null;
            try
            {
                IPipelineContext context = ctx.Cast<TC>();
                this.cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var token = this.cancellation.Token;
                foreach (var step in this.steps)
                {
                    try
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                        context = await step(context);
                    }
                    catch (Exception err)
                    {
                        if (this.exceptionHandler != null)
                        {
                            context = await this.exceptionHandler(context, err);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                this.cancellation?.Dispose();
                result = context.Cast<TC>();
                result.Invoke(PipeEventArgs.Completed(result));
                return result;
            }
            finally
            {
                if (this.finalBlock != null)
                {
                    if (result is TC r)
                    {
                        await this.finalBlock(r);
                    }
                }
            }
        }


        #endregion
        public Pipeline<TC, TInput> Except(Func<TC, Exception, ValueTask<TC>> step)
        {
            this.exceptionHandler = async (ctx, err) =>
            {
                return await step(ctx.Cast<TC>(false), err);
            };
            return this;
        }
        public Pipeline<TC, TInput> Finally(Func<TC, ValueTask> block)
        {
            this.finalBlock = ctx => block(ctx.Cast<TC>(false));
            return this;
        }

        internal IPipeContext<T> CreateContext<T>(T inpput)
        {
            return new PipeContext<T>(inpput, this.serviceProvider.CreateScope());
        }

        public ValueTask<TC> Run(IPipeContext<TInput> ctx, CancellationToken cancellationToken)
        {
            return ListMode ? RunWithList(ctx, cancellationToken) : RunWithPipe(ctx);
        }

        public async ValueTask<TC> Run(TInput input, CancellationToken cancellationToken = default)
        {
            using (var ctx = new PipeContext<TInput>(input, this.serviceProvider.CreateScope()))
            {
                return await this.Run(ctx, cancellationToken);
            }
        }
        public async ValueTask<TO> Run<TO>(TInput input, CancellationToken cancellationToken = default)
        {
            using (var ctx = new PipeContext<TInput>(input, this.serviceProvider.CreateScope()))
            {
                var result = await this.Run(ctx, cancellationToken);
                return (TO)(result.State);
            }
        }

        public PipelineQueue<TC, TInput> GetQueue(CancellationToken token = default, int numberOfThreads = 1)
        {
            var result = new PipelineQueue<TC, TInput>(this, token, numberOfThreads);
            _ = result.Run();
            return result;

        }

    }
    public class Pipeline<TC, TInput, TOutput> : Pipeline<TC, TInput> where TC : PipeContext<TOutput>
    {
        public Pipeline(string name = null, IServiceProvider serviceProvider = null)
            : base(name, serviceProvider)
        {
        }
        public new ValueTask<TOutput> Run(TInput input, CancellationToken cancellationToken = default)
        {
            return this.Run<TOutput>(input, cancellationToken);
        }
    }
    public class Pipeline<TState> : Pipeline<PipeContext<TState>, TState>
    {
        public Pipeline(string name = null, IServiceProvider serviceProvider = null, Func<IPipelineContext, object, ValueTask<IPipelineContext>> pipe = null, Func<IPipelineContext, Exception, ValueTask<IPipelineContext>> exceptionHandler = null, Func<IPipelineContext, ValueTask> finalBlock = null)
            : base(name, serviceProvider, pipe, null, exceptionHandler, finalBlock)
        {
        }
    }

}
