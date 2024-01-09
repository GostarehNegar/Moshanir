using GN.Library.Helpers;
using GN.Library.TaskScheduling;
using Mapna.Transmittals.Exchange.GhodsNiroo.Incoming;
using Mapna.Transmittals.Exchange.GhodsNiroo.Incoming.Steps;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.GhodsNiroo
{
    public interface IGhodsNirooIncomingQueue
    {
        void Enqueue(IncomingTransmittalRequest request);
    }
    class GhodsNirooIncomingQueue : BackgroundMultiBlockingTaskHostedService, IGhodsNirooIncomingQueue
    {
        private readonly IServiceProvider serviceProvider;
        private readonly GhodsNirooTransmittalOptions options;
        private ConcurrentDictionary<string, IncomingTransmittalRequest> items = new ConcurrentDictionary<string, IncomingTransmittalRequest>();

        public GhodsNirooIncomingQueue(IServiceProvider serviceProvider, GhodsNirooTransmittalOptions options) : base(1)
        {
            this.serviceProvider = serviceProvider;
            this.options = options;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public IncomingTransmittalContext DoEnqueue(IncomingTransmittalContext context)
        {
            base.Enqueue(async token =>
            {
                Task<IncomingTransmittalContext> task = null;
                var max_trials = 3;
                for (var trial = 0; trial < max_trials; trial++)
                {
                    task = WithAsync<IncomingTransmittalContext>
                    .Setup()
                    .First(IncomingSteps.Validate)
                    .Then(IncomingSteps.EnsureJob)
                    .Then(IncomingSteps.DownloadLetter)
                    .Then(IncomingSteps.DownloadFiles)
                    .Then(IncomingSteps.SendResultFeedBack)
                    .Run(context.WithCancellationToken(CancellationTokenSource.CreateLinkedTokenSource(token)));
                    try
                    {
                        var result = await task;
                        break;
                    }
                    catch (Exception err)
                    {
                        if (err is IncomingException exp && !exp.Retryable)
                        {
                            context.Log(LogLevel.Error,
                                $"An unrecoverable error occured while trying to receive Transmittal '{context.Request}' the operation will be aborted. Err: '{exp.GetBaseException().Message}' ");
                        }
                        else
                        {
                            context.Log(LogLevel.Warning,
                                $"An error occured while trying to receive Transmittal '{context}'. We will try {max_trials - trial - 1} more times.");
                            //context.IncrementTrial();
                        }
                        //context.SetResult(task);
                    }
                    await Task.Delay(trial * 60 * 1000);
                    //context = new IncomingTransmittalContext(this.serviceProvider, context.Data)
                    //    .WithCancellationToken(CancellationTokenSource.CreateLinkedTokenSource(token));
                }
                if (task.IsCanceled)
                {
                    await context.GetSPContext().SetJobFailed(context.Id, "Canceled");
                }
                else if (task.IsFaulted)
                {
                    await context.GetSPContext().SetJobFailed(context.Id, task.Exception.GetBaseException().Message);
                }
                else if (task.IsCompleted)
                {
                    await context.GetSPContext().SetJobCompleted(context.Id, "");
                }
                //context.SetResult(task);
                context.Dispose();

            });
            return context;

        }


        public void Enqueue(IncomingTransmittalRequest request)
        {
            if (this.items.TryGetValue(request.TR_NO, out var res))
            {
                return;
            }
            this.items.GetOrAdd(request.TR_NO, request);
            var ctx = new IncomingTransmittalContext(this.serviceProvider, this.options, request);
            this.DoEnqueue(ctx);
        }
    }
}
