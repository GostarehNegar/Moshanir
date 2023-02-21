using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.TaskScheduling
{
    class BackgroundBlockingTaskHostedService : BackgroundService
    {
        private BlockingCollection<Func<CancellationToken, Task>> queue = new BlockingCollection<Func<CancellationToken, Task>>();
        private int count;

        public bool Enqueue(Func<CancellationToken, Task> producer)
        {
            count++;
            return this.queue.TryAdd(producer);
        }
        public int Count => this.count;
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {

                while (!stoppingToken.IsCancellationRequested)
                {
                    if (this.queue.TryTake(out var item, 2000, stoppingToken))
                    {
                        if (item != null)
                        {
                            try
                            {
                                await item.Invoke(stoppingToken);
                            }
                            catch (Exception err)
                            {
                                //throw;
                            }
                            count--;
                        }
                    }
                }
            });
        }
    }
    public class BackgroundMultiBlockingTaskHostedService : IHostedService
    {
        private List<BackgroundBlockingTaskHostedService> queues;

        public BackgroundMultiBlockingTaskHostedService(int no = 1)
        {
            this.queues = Enumerable.Range(0, no)
                .Select(x => new BackgroundBlockingTaskHostedService())
                .ToList();
        }

        public virtual bool Enqueue(Func<CancellationToken, Task> producer)
        {

            return this.queues.OrderBy(x => x.Count).First().Enqueue(producer);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(this.queues.Select(x => x.StartAsync(cancellationToken)));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(this.queues.Select(x => x.StopAsync(cancellationToken)));
        }


    }
}
