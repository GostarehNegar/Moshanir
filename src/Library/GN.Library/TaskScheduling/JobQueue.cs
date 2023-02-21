using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace GN.Library.TaskScheduling
{
    public class JobQueue
    {

    }
    //public class AsyncQueue<T>
    //{
    //    protected ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
    //    private SemaphoreSlim _signal = new SemaphoreSlim(0);
    //    public void Enqueue(T value)
    //    {
    //        queue.Enqueue(value);
    //        _signal.Release();
    //    }
    //    public async Task<T> DequeueAsync(int timeOut, CancellationToken cancellationToken)
    //    {
    //        T result = default(T);
    //        var success = await _signal.WaitAsync(timeOut, cancellationToken);
    //        if (success)
    //            this.queue.TryDequeue(out result);


    //        return result;
    //    }
    //}

    

    /// <summary>
    /// Deprecated please QueuedHostedServices
    /// </summary>
    /// <typeparam name="T"></typeparam>
    //public class JobQueue<T> : HostedService, IHostedService
    //{
    //    protected ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
    //    protected Func<T, Task> worker;
    //    private ManualResetEventSlim actionQueueNotEmptyEvent = new ManualResetEventSlim(false);
    //    private int timeOut;

    //    public JobQueue(Func<T, Task> worker, int timeOut = 1000)
    //    {
    //        this.worker = worker;
    //        this.timeOut = timeOut;
    //    }
    //    public void Enqueue(T value)
    //    {
    //        queue.Enqueue(value);
    //        if (queue.Count > 0)
    //            this.actionQueueNotEmptyEvent.Set();
    //    }

    //    protected async Task<bool> Wait(CancellationToken cancellationToken, int miliseconds = 1000)
    //    {
    //        var result = false;
    //        try
    //        {
    //            await Task.WhenAny(Task.Run(() =>
    //                {
    //                    this.actionQueueNotEmptyEvent.Wait(miliseconds);
    //                })
    //                , Task.Delay(miliseconds, cancellationToken))
    //            .ConfigureAwait(false);
    //        }
    //        catch { }
    //        return result = this.queue.Count > 0;
    //    }
    //    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    //    {
    //        while (!cancellationToken.IsCancellationRequested)
    //        {
    //            await Task.FromResult(true).ConfigureAwait(false);
    //            await this.Wait(cancellationToken, this.timeOut);
    //            while (this.queue.TryDequeue(out var item) && item != null)
    //            {
    //                if (cancellationToken.IsCancellationRequested)
    //                    break;
    //                await this.worker.Invoke(item).ConfigureAwait(false);
    //            }
    //            if (this.queue.Count == 0)
    //                this.actionQueueNotEmptyEvent.Reset();
    //        }
    //    }
    //}

    //public class ActionQueue<T> : HostedService, IHostedService
    //{
    //    protected ConcurrentQueue<Func<Task>> queue = new ConcurrentQueue<Func<Task>>();
    //    private ManualResetEventSlim actionQueueNotEmptyEvent = new ManualResetEventSlim(false);
    //    private int timeOut;

    //    public ActionQueue(int timeOut = 1000)
    //    {
    //        this.timeOut = timeOut;
    //    }
    //    public void Enqueue(Func<Task> item)
    //    {
    //        queue.Enqueue(item);
    //        if (queue.Count > 0)
    //            this.actionQueueNotEmptyEvent.Set();
    //    }

    //    protected async Task<bool> Wait(CancellationToken cancellationToken, int miliseconds = 1000)
    //    {
    //        var result = false;
    //        try
    //        {
    //            await Task.WhenAny(Task.Run(() =>
    //            {
    //                this.actionQueueNotEmptyEvent.Wait(miliseconds);
    //            })
    //                , Task.Delay(miliseconds, cancellationToken))
    //            .ConfigureAwait(false);
    //        }
    //        catch { }
    //        return result = this.queue.Count > 0;
    //    }
    //    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    //    {
    //        while (!cancellationToken.IsCancellationRequested)
    //        {
    //            await Task.FromResult(true).ConfigureAwait(false);
    //            await this.Wait(cancellationToken, this.timeOut);
    //            while (this.queue.TryDequeue(out var item) && item != null)
    //            {
    //                if (cancellationToken.IsCancellationRequested)
    //                    break;
    //                await item?.Invoke();
    //            }
    //            if (this.queue.Count == 0)
    //                this.actionQueueNotEmptyEvent.Reset();
    //        }
    //    }
    //}
    //public class ActionQueue : HostedService, IHostedService
    //{
    //    protected ConcurrentQueue<Func<Task>> queue = new ConcurrentQueue<Func<Task>>();
    //    private ManualResetEventSlim actionQueueNotEmptyEvent = new ManualResetEventSlim(false);
    //    private int timeOut;

    //    public ActionQueue(int timeOut = 1000)
    //    {
    //        this.timeOut = timeOut;
    //    }
    //    public void Enqueue(Func<Task> item)
    //    {
    //        queue.Enqueue(item);
    //        if (queue.Count > 0)
    //            this.actionQueueNotEmptyEvent.Set();
    //    }
    //    public int GetCount()
    //    {
    //        return this.queue.Count;
    //    }

    //    protected async Task<bool> Wait(CancellationToken cancellationToken, int miliseconds = 1000)
    //    {
    //        var result = false;
    //        try
    //        {
    //            await Task.WhenAny(Task.Run(() =>
    //            {
    //                this.actionQueueNotEmptyEvent.Wait(miliseconds);
    //            })
    //                , Task.Delay(miliseconds, cancellationToken))
    //            .ConfigureAwait(false);
    //        }
    //        catch { }
    //        return result = this.queue.Count > 0;
    //    }
    //    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    //    {
    //        while (!cancellationToken.IsCancellationRequested)
    //        {
    //            await Task.FromResult(true).ConfigureAwait(false);
    //            await this.Wait(cancellationToken, this.timeOut);
    //            while (this.queue.TryDequeue(out var item) && item != null)
    //            {
    //                if (cancellationToken.IsCancellationRequested)
    //                    break;
    //                try
    //                {
    //                    if (item != null)
    //                        await item.Invoke().ConfigureAwait(false);
    //                    //await Task.Delay(1000);
    //                }
    //                catch { }
    //            }
    //            if (this.queue.Count == 0)
    //                this.actionQueueNotEmptyEvent.Reset();
    //        }
    //    }
    //}

    //public class ActionQueueEx_deprecated : HostedService, IHostedService
    //{

    //    private BlockingCollection<Func<Task>> queue = new BlockingCollection<Func<Task>>();
    //    private int timeOut;
    //    public ActionQueueEx_deprecated(int timeOut = 1000)
    //    {
    //        this.timeOut = timeOut;
    //    }
    //    public void Enqueue(Func<Task> item)
    //    {
    //        if (this._cts.IsCancellationRequested)
    //        {

    //        }
    //        queue.Add(item);

    //    }
    //    public int GetCount()
    //    {

    //        return this.queue.Count;
    //    }

    //    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    //    {
    //        return Task.Run(async () =>
    //        {
    //            await Task.Delay(100);
    //            while (!cancellationToken.IsCancellationRequested)
    //            {
    //                while (this.queue.TryTake(out var f, this.timeOut, cancellationToken))
    //                {
    //                    try
    //                    {
    //                        await (f?.Invoke() ?? Task.CompletedTask);
    //                    }
    //                    catch
				//		{ }
    //                }
    //            }


    //        });
    //    }
    //}

    /// <summary>
    /// Deprecated please QueuedHostedServices
    /// </summary>

    //public class ActionQueueExEx : IHostedService
    //{
    //    private List<QueuedHostedService> queues = new List<QueuedHostedService>();
    //    public ActionQueueExEx(int numberOfWorkingQueues = 1)
    //    {
    //        for (var i = 0; i < numberOfWorkingQueues; i++)
    //        {
    //            queues.Add(new QueuedHostedService());
    //        }
    //    }
    //    public void Enqueue(Func<Task> item)
    //    {
    //        this.queues.OrderBy(x => x.GetCount()).First().TaskQueue.Enqueue(c=> item());
    //    }

    //    public Task StartAsync(CancellationToken cancellationToken)
    //    {
    //        return Task.WhenAll(this.queues.Select(x => x.StartAsync(cancellationToken)));
    //    }

    //    public Task StopAsync(CancellationToken cancellationToken)
    //    {
    //        return Task.WhenAll(this.queues.Select(x => x.StopAsync(cancellationToken)));
    //    }

    //}
}
