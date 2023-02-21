using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace GN.Library.TaskScheduling
{


	/// <summary>
	/// Refernce : https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-5.0&tabs=visual-studio
	/// </summary>
	internal class AsyncTaskQueue_
	{
		private ConcurrentQueue<Func<CancellationToken, Task>> _workItems =
			new ConcurrentQueue<Func<CancellationToken, Task>>();
		private SemaphoreSlim _signal = new SemaphoreSlim(0);

		public void Enqueue(Func<CancellationToken, Task> workItem)
		{
			if (workItem == null)
			{
				throw new ArgumentNullException(nameof(workItem));
			}
			_workItems.Enqueue(workItem);
			_signal.Release();
		}

		public async Task<Func<CancellationToken, Task>> DequeueAsync(
			CancellationToken cancellationToken, int timeOut = Timeout.Infinite)
		{
			await _signal.WaitAsync(timeOut,cancellationToken);
			_workItems.TryDequeue(out var workItem);
            
			return workItem;
		}
        public int GetCount()
        {

            return this._workItems.Count;
        }
    }
    public class AsyncQueue<T>
    {
        private ConcurrentQueue<T> _workItems =
            new ConcurrentQueue<T>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void Enqueue(T workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
            _signal.Release();
        }
        public async Task<T> DequeueAsync(CancellationToken cancellationToken, int timeout = Timeout.Infinite)
        {
            await _signal.WaitAsync(timeout, cancellationToken);
            _workItems.TryDequeue(out var workItem);
            return workItem;
        }
        public int GetCount()
        {
            return this._workItems.Count;
        }
    }
    
    public class AsyncTaskQueue : AsyncQueue<Func<CancellationToken, Task>>
	{

	}

    public class BackgroundTaskQueuedHostedService : BackgroundService
    {
        private readonly ILogger<BackgroundTaskQueuedHostedService> _logger;

        public BackgroundTaskQueuedHostedService(ILogger<BackgroundTaskQueuedHostedService> logger=null)
        {
            TaskQueue = new AsyncTaskQueue();
            _logger = logger;
        }

        public AsyncTaskQueue TaskQueue { get; }
        public int GetCount() => TaskQueue.GetCount();
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //_logger.LogInformation(
            //    $"Queued Hosted Service is running.{Environment.NewLine}" +
            //    $"{Environment.NewLine}Tap W to add a work item to the " +
            //    $"background queue.{Environment.NewLine}");

            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem =
                    await TaskQueue.DequeueAsync(stoppingToken);
                try
                {
                    await workItem(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex,
                        "Error occurred executing {WorkItem}.", nameof(workItem));
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger?.LogInformation("Queued Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    
        public void Enqueue(Func<CancellationToken,Task> item)
		{
            this.TaskQueue.Enqueue(item);
		}
    }
    public class BackgroundMultiTaskQueuedHostedService : IHostedService
    {
        private List<BackgroundTaskQueuedHostedService> queues = new List<BackgroundTaskQueuedHostedService>();
        public BackgroundMultiTaskQueuedHostedService(int numberOfWorkingQueues = 1)
        {
            for (var i = 0; i < numberOfWorkingQueues; i++)
            {
                queues.Add(new BackgroundTaskQueuedHostedService());
            }
        }
        public void Enqueue(Func<Task> item)
        {
            this.queues.OrderBy(x => x.GetCount()).First().TaskQueue.Enqueue(c => item());
        }
        public void Enqueue(Func<CancellationToken, Task> item)
        {
            this.queues.OrderBy(x => x.GetCount()).First().TaskQueue.Enqueue(item);
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
