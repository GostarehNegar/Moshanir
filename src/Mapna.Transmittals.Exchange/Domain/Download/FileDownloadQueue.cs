using GN.Library.TaskScheduling;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using GN.Library.Helpers;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.IO;
using System.Net;
using System.Threading;
using System;
using System.Linq;
using GN.Library.Messaging;
using Mapna.Transmittals.Exchange.Services.Queues.Download.Steps;
using Mapna.Transmittals.Exchange.Services.Queues;

namespace Mapna.Transmittals.Exchange.Internals
{
    public interface IFileDownloadQueue
    {
        //FileDownloadContext Enqueue(string url, string destination, bool withProgressSupport = false);
        FileDownloadContext Enqueue(string url, string destination, Action<FileDownloadContext> configure = null);
        void Cancel(Func<FileDownloadContext, bool> predicate);
        IEnumerable<FileDownloadContext> GetJobs();
    }
    class FileDownloadQueue : BackgroundMultiBlockingTaskHostedService, IFileDownloadQueue
    {

        public FileDownloadQueue(ILogger<FileDownloadQueue> logger, IServiceProvider serviceProvider, IMessageBus bus) : base(4)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.bus = bus;
        }
        public ConcurrentDictionary<string, FileDownloadContext> urls = new ConcurrentDictionary<string, FileDownloadContext>();
        private readonly ILogger<FileDownloadQueue> logger;
        private readonly IServiceProvider serviceProvider;
        private readonly IMessageBus bus;
       
        internal void SetResult(Task<FileDownloadContext> ctx, FileDownloadContext result)
        {
            if (this.urls.TryRemove(result.Url, out var _t))
            {
                //_t.Dispose();
            }
            if (ctx.IsFaulted)
            {
                if (result.IsRetryable(ctx.Exception.GetBaseException()))
                {
                    var span = result.IncrementTrials();
                    result.SendLog(LogLevel.Warning,
                        $"Failed to Download File. '{result.FileSubmitModel?.FileName}', url: '{result.Url}'. We will retry in '{span / 1000}' Seconds. \r\n Error:'{ctx.Exception.GetBaseException().Message}' ");
                    Task.Delay(span, result.CancellationToken)
                        .ContinueWith(x =>
                        {
                            if (!x.IsCanceled)
                            {
                                this.Enqueue(result);
                            }
                        });
                }
                else
                {
                    result.CompletionSource.SetException(ctx.Exception);
                    _t?.Dispose();
                }
            }
            else if (ctx.IsCanceled)
            {
                result.CompletionSource.SetCanceled();
                result.InvokeCanceled();
                _t?.Dispose();
            }
            else if (ctx.IsCompleted)
            {
                result.CompletionSource.SetResult(result);
                result.InvokeCompelted();
                result.SendLog(LogLevel.Information,
                    $"File :'{result.Url}' Successfully Downloaded Location:'{result.Destination}'");
                _t?.Dispose();
            }
        }
        private Task DoEnqueue(CancellationToken token, FileDownloadContext result)
        {
            return
                    WithAsync<FileDownloadContext>.Setup()
                    .First(ctx => ctx.WithServiceProvide(ctx.ServiceProvider == null ? this.serviceProvider : ctx.ServiceProvider).WithToken(token))
                    
                    .Then(DownloadSteps.DownloadFile)
                    .Then(DownloadSteps.CheckFileMetadata)
                    .Run(result)
                    .ContinueWith(t => SetResult(t, result));
        }
        private FileDownloadContext Enqueue(FileDownloadContext context)
        {
            if (!this.urls.TryAdd(context.Url, context) || !base.Enqueue(t => DoEnqueue(t, context)))
            {
                throw new Exception("Failed to add to queue");
            }
            return context;
        }
        public FileDownloadContext Enqueue(string url, string destination, Action<FileDownloadContext> configure = null)
        {
            if (urls.TryGetValue(url, out var result))
                return result;
            result = new FileDownloadContext(url, destination);
            configure?.Invoke(result);
            var res= Enqueue(result);
            return res;

        }

        public void Cancel(Func<FileDownloadContext, bool> predeicate)
        {
            foreach (var f in this.urls.Values)
            {
                if (predeicate == null || predeicate(f))
                {
                    f.Cancel();
                }
            }
        }

        public IEnumerable<FileDownloadContext> GetJobs()
        {
            return this.urls.Values.ToArray();
        }
    }
}
