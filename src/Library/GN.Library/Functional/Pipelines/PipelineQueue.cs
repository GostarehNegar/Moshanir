using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Functional.Pipelines
{
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

        public IPipelineContext Enqueue(TInput item)
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
