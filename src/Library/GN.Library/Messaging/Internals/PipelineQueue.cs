using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using GN.Library.Messaging.Pipeline;

namespace GN.Library.Messaging.Internals
{
    class PipelineQueue
    {
        public List<BlockingCollection<PipelineContext>> queues;
        public PipelineQueue(int numberOfQueues)
        {
            queues = new List<BlockingCollection<PipelineContext>>();
            queues.AddRange(Enumerable.Range(1, numberOfQueues).Select(x => new BlockingCollection<PipelineContext>()));
        }
        public bool Started { get; private set; }
        public Task Enqueue(PipelineContext pipeline)
        {
            lock (queues)
            {
                var min = this.queues.FirstOrDefault(x => x.Count == this.queues.Min(y => y.Count)) ?? this.queues.First();
                min.Add(pipeline);
            }
            return pipeline.CompletionTask;
        }
        public Task Start(CancellationToken token)
        {
            var result = new List<Task>();
            result.AddRange(queues.Select(x =>
            {
                return Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        var ctx = x.Take(token);
                        try
                        {
                            _ =  ctx.Invoke().ConfigureAwait(false);
                        }
                        catch (Exception err)
                        {
                            ctx.RaiseError(err);
                        }
                    }
                });
            }));
            this.Started = true;
            return Task.WhenAll(result);
        }
    }
}
