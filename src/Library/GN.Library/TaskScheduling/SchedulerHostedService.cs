using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GN.Library.TaskScheduling.Corn;

namespace GN.Library.TaskScheduling
{
    public interface ITaskSchedueler
    {
        void Schedule(Func<CancellationToken, Task> task, string schedule, int? occurences = null);
        void Schedule(Func<CancellationToken, Task> task, DateTime schedule);

    }
    public class SchedulerHostedService : HostedService
    {
        public event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        private List<SchedulerTaskWrapper> _scheduledTasks = new List<SchedulerTaskWrapper>();
        private class ScheduledTask : IScheduledTask
        {
            public string Schedule { get; set; }

            public string Name { get; set; }
            public Func<CancellationToken, Task> Task { get; set; }

            public Task ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task(cancellationToken);
            }
        }

        public SchedulerHostedService(IEnumerable<IScheduledTask> scheduledTasks)
        {
            var referenceTime = DateTime.UtcNow;

            foreach (var scheduledTask in scheduledTasks)
            {
                _scheduledTasks.Add(new SchedulerTaskWrapper
                {
                    Schedule = CrontabSchedule.Parse(scheduledTask.Schedule),
                    Task = scheduledTask,
                    NextRunTime = referenceTime
                });
            }
        }
        public void Schedule(Func<CancellationToken, Task> task, string schedule, int? occurences = null)
        {
            var _schedule = CrontabSchedule.Parse(schedule);
            var nextRun = _schedule.GetNextOccurrence(DateTime.UtcNow);
            var t = new SchedulerTaskWrapper
            {
                Schedule = _schedule,
                MaxCount = occurences,
                NextRunTime = nextRun,
                Task = new ScheduledTask
                {
                    Task = task,
                    Schedule = schedule,
                }
            };
            lock (this._scheduledTasks)
            {
                this._scheduledTasks.Add(t);
            }
        }
        void Schedule(Func<CancellationToken, Task> task, DateTime schedule)
        {
            var _schedule = CrontabSchedule.Parse(CronHelper.Every2Minutes);
            var nextRun = schedule;
            var t = new SchedulerTaskWrapper
            {
                Schedule = _schedule,
                MaxCount = 1,
                NextRunTime = nextRun,
                Task = new ScheduledTask
                {
                    Task = task,
                    Schedule = CronHelper.Every2Minutes,
                }
            };
            lock (this._scheduledTasks)
            {
                this._scheduledTasks.Add(t);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await ExecuteOnceAsync(cancellationToken);

                lock (_scheduledTasks)
                {
                    _scheduledTasks = this._scheduledTasks.Where(x => !x.MaxCount.HasValue || x.MaxCount.Value < x.CallCount).ToList();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            var taskFactory = new TaskFactory(TaskScheduler.Current);
            var referenceTime = DateTime.UtcNow;

            var tasksThatShouldRun = _scheduledTasks.Where(t => t.ShouldRun(referenceTime)).ToList();

            foreach (var taskThatShouldRun in tasksThatShouldRun)
            {
                taskThatShouldRun.Increment();
                await taskFactory.StartNew(
                    async () =>
                    {
                        try
                        {
                            await taskThatShouldRun.Task.ExecuteAsync(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            var args = new UnobservedTaskExceptionEventArgs(
                                ex as AggregateException ?? new AggregateException(ex));

                            UnobservedTaskException?.Invoke(this, args);

                            if (!args.Observed)
                            {
                                throw;
                            }
                        }
                    },
                    cancellationToken);
            }
        }

        private class SchedulerTaskWrapper
        {
            public CrontabSchedule Schedule { get; set; }
            public IScheduledTask Task { get; set; }

            public DateTime LastRunTime { get; set; }
            public DateTime NextRunTime { get; set; }
            public int CallCount { get; set; }
            public int? MaxCount { get; set; }

            public void Increment()
            {
                CallCount++;
                LastRunTime = NextRunTime;
                NextRunTime = Schedule.GetNextOccurrence(NextRunTime);
            }

            public bool ShouldRun(DateTime currentTime)
            {
                return NextRunTime < currentTime && LastRunTime != NextRunTime && (!MaxCount.HasValue || MaxCount.Value > CallCount);
            }
        }
    }
}