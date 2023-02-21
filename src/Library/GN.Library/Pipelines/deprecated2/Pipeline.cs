using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace GN.Library.Pipelines.deprecated2
{
    public class StepInfo
    {
        public string Name { get; set; }
    }
    public interface IPipeline<TC, TInput> where TC : PipelineContext
    {
        string Name { get; }
        ValueTask<TC> Run(TInput input);
        PipelineQueue<TC, TInput> GetQueue(CancellationToken token = default, int numberOfThreads = 1);
    }
    public class Pipeline<TC, TInput> where TC : PipelineContext
    {
        const bool ListMode = true;
        private Func<PipelineContext, object, ValueTask<PipelineContext>> pipe;
        private List<Func<PipelineContext, ValueTask<PipelineContext>>> steps;
        private Func<PipelineContext, Exception, ValueTask<PipelineContext>> exceptionHandler;
        private Func<PipelineContext, ValueTask> finalBlock;
        private readonly IServiceProvider serviceProvider;
        public string Name { get; protected set; }
        public Pipeline(
            string name,
            IServiceProvider serviceProvider = null,
            Func<PipelineContext, object, ValueTask<PipelineContext>> pipe = null,
            List<Func<PipelineContext, ValueTask<PipelineContext>>> steps = null,
            Func<PipelineContext, Exception, ValueTask<PipelineContext>> exceptionHandler = null,
            Func<PipelineContext, ValueTask> finalBlock = null)
        {
            var stateType = typeof(TC).GetGenericArguments().Length == 1 ? typeof(TC).GetGenericArguments()[0] : typeof(object);
            this.Name = name ?? $"Pipe<{stateType.Name}";
            this.pipe = pipe ?? ((ctx, h) => new ValueTask<PipelineContext>(ctx));
            this.steps = steps ?? new List<Func<PipelineContext, ValueTask<PipelineContext>>>();
            this.serviceProvider = serviceProvider ?? new ServiceCollection().BuildServiceProvider();
            this.exceptionHandler = exceptionHandler;
            this.finalBlock = finalBlock;
        }
        internal Pipeline<TC, TInput> DoAddStep(Func<TC, ValueTask<TC>> step, Action<StepInfo> configure)
        {
            return ListMode ? DoAddStepWithList(step, configure) : DoAddStepWithPipe(step, configure);
        }
        internal Pipeline<TN, TInput> DoCast<TN>(Func<TC, ValueTask<TN>> step, Action<StepInfo> configure) where TN : PipelineContext
        {
            return ListMode ? DoCastWithList(step, configure) : DoCastWithPipe<TN>(step, configure);
        }
        #region pipe
        private Pipeline<TC, TInput> DoAddStepWithPipe(Func<TC, ValueTask<TC>> step, Action<StepInfo> configure)
        {
            var pipe = this.pipe;
            var stepInfo = new StepInfo
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
                    res.Invoke(PipelineEventArgs.StepStart(res, stepInfo));
                    return await step(res);
                }
                catch (Exception err)
                {
                    input.Invoke(PipelineEventArgs.Error(input, stepInfo, err));
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
        private Pipeline<TN, TInput> DoCastWithPipe<TN>(Func<TC, ValueTask<TN>> step, Action<StepInfo> configure) where TN : PipelineContext
        {
            var pipe = this.pipe;
            var stepInfo = new StepInfo
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
                    res.Invoke(PipelineEventArgs.StepStart(res, stepInfo));
                    return await step(res);
                }
                catch (Exception err)
                {
                    input.Invoke(PipelineEventArgs.Error(input, stepInfo, err));
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
        public async ValueTask<TC> RunWithPipe(PipelineContext<TInput> ctx)
        {
            TC result = null;
            try
            {
                result = (await this.pipe(ctx, this)).Cast<TC>();
                result.Invoke(PipelineEventArgs.Completed(result));

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
        private Pipeline<TC, TInput> DoAddStepWithList(Func<TC, ValueTask<TC>> step, Action<StepInfo> configure)
        {
            var pipe = this.pipe;
            var stepInfo = new StepInfo
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
        private Pipeline<TN, TInput> DoCastWithList<TN>(Func<TC, ValueTask<TN>> step, Action<StepInfo> configure) where TN : PipelineContext
        {
            var stepInfo = new StepInfo
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

        public async ValueTask<TC> RunWithList(PipelineContext<TInput> ctx, CancellationToken cancellationToken)
        {
            TC result = null;
            try
            {
                PipelineContext context = ctx;
                foreach (var step in this.steps)
                {
                    try
                    {
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
                result = context.Cast<TC>();
                result.Invoke(PipelineEventArgs.Completed(result));
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

        internal PipelineContext<T> CreateContext<T>(T inpput)
        {
            return new PipelineContext<T>(inpput, this.serviceProvider.CreateScope());
        }

        public ValueTask<TC> Run(PipelineContext<TInput> ctx, CancellationToken cancellationToken)
        {
            return ListMode ? RunWithList(ctx,cancellationToken) : RunWithPipe(ctx);
        }

        public async ValueTask<TC> Run(TInput input, CancellationToken cancellationToken= default)
        {
            using (var ctx = new PipelineContext<TInput>(input, this.serviceProvider.CreateScope()))
            {
                return await this.Run(ctx, cancellationToken);
            }
        }


        public PipelineQueue<TC, TInput> GetQueue(CancellationToken token = default, int numberOfThreads = 1)
        {
            var result = new PipelineQueue<TC, TInput>(this, token, numberOfThreads);
            _ = result.Run();
            return result;

        }

    }

    public class Pipeline<TState> : Pipeline<PipelineContext<TState>, TState>
    {
        public Pipeline(string name = null, IServiceProvider serviceProvider = null, Func<PipelineContext, object, ValueTask<PipelineContext>> pipe = null, Func<PipelineContext, Exception, ValueTask<PipelineContext>> exceptionHandler = null, Func<PipelineContext, ValueTask> finalBlock = null)
            : base(name, serviceProvider, pipe, null, exceptionHandler, finalBlock)
        {
        }
    }

    public class PipelineQueue<TC, TInput> : IDisposable where TC : PipelineContext
    {
        private List<Queue> queues;
        private readonly Pipeline<TC, TInput> pipe;
        private CancellationTokenSource cancellationTokenSource;
        public PipelineQueue(Pipeline<TC, TInput> pipe, CancellationToken token, int no = 1)
        {
            this.pipe = pipe;
            this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            this.queues = Enumerable.Range(0, no)
                .Select(x => new Queue())
                .ToList();

        }
        private BlockingCollection<TC> channel = new BlockingCollection<TC>();
        internal class Queue : BlockingCollection<Func<ValueTask<TC>>>
        {
            public int ItemCount;

            public Task Run(CancellationToken token, BlockingCollection<TC> channel)
            {
                return Task.Run(async () =>
                {
                    foreach (var item in this.GetConsumingEnumerable(token))
                    {
                        try
                        {
                            var f = await item();
                            ItemCount--;
                            channel.Add(f);
                            f.Dispose();
                        }
                        catch (TaskCanceledException)
                        {

                        }
                    }
                });


            }
            public void AddEx(Func<ValueTask<TC>> item)
            {
                this.Add(item);
                ItemCount++;
            }
        }

        public PipelineContext Enqueue(TInput item)
        {
            var ctx = this.pipe.CreateContext<TInput>(item);
            var q = queues.OrderBy(x => x.ItemCount).First();
            q.AddEx(() =>
            {
                var p = item;
                return this.pipe.Run(ctx, default);
            });
            return ctx;
        }
        public Task Run()
        {
            //this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            return Task.WhenAll(
                this.queues
                    .Select(x => x.Run(this.cancellationTokenSource.Token, channel))
                    .ToArray());

        }
        public IEnumerable<TC> GetConsumingEnumerable()
        {
            return channel.GetConsumingEnumerable(this.cancellationTokenSource.Token);
        }
        public void Stop()
        {
            //this.cancellationTokenSource.Cancel();
            this.Dispose();

        }

        public void Dispose()
        {
            this.cancellationTokenSource?.Cancel();
            this.queues.ForEach(x => x.Dispose());
            this.channel.Dispose();

        }
    }

}
