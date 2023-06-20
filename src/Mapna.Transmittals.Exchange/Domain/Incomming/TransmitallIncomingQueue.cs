using GN.Library.Helpers;
using GN.Library.Messaging;
using GN.Library.TaskScheduling;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SharePoint.Client;
using Mapna.Transmittals.Exchange.Services;
using System.Linq;
using Mapna.Transmittals.Exchange.Services.Queues.Incomming.Steps;
using Mapna.Transmittals.Exchange.Services.Queues;
using Mapna.Transmittals.Exchange.Internals;
using System.Net.Http;

namespace Mapna.Transmittals.Exchange.Services.Queues.Incomming
{
    public interface IIncommingQueue
    {
        IncommingTransmitalContext Enqueue(TransmittalSubmitModel transmittal, Action<IncommingTransmitalContext> configure = null);
    }
    class TransmitallIncomingQueue : BackgroundMultiBlockingTaskHostedService, IIncommingQueue
    {
        private ConcurrentDictionary<string, IncommingTransmitalContext> items = new ConcurrentDictionary<string, IncommingTransmitalContext>();
        private readonly ILogger<TransmitallIncomingQueue> logger;
        private readonly IServiceProvider serviceProvider;
        private readonly IMessageBus bus;
        private readonly IFileDownloadQueue downloader;

        public TransmitallIncomingQueue(ILogger<TransmitallIncomingQueue> logger, IServiceProvider serviceProvider, IMessageBus bus, IFileDownloadQueue downloader) : base(1)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.bus = bus;
            this.downloader = downloader;
        }

        public async Task<IncommingTransmitalContext> GetOrAddTransmittal(IncommingTransmitalContext ctx)
        {
            var transmittal = ctx.Transmittal;
            var count = await ctx.GetRepository().CountTransmitalls();
            ctx.TransmittalItem = await ctx.GetRepository().GetOrAddTransmittal(ctx.Transmittal.TR_NO, t =>
             {
                 t.ReferenceNumber = transmittal.TR_NO;
                 t.Title = $"TRANS-{count + 1}";
             });
            return ctx;
        }
        private Task StartDownload(TransmittalFileSubmitModel file, IncommingTransmitalContext context)
        {
            var downloader = this.downloader;// context.ServiceProvider.GetService<IFileDownloadQueue>();
            context.State.SetFileState(file.Url, "InProgress");
            //var fileName = context.GetDestinationFileName(file);
            var fileName = context.GetSharePointDestinationPath(file);
            var result = downloader.Enqueue(file.Url, context.GetDestinationFileName(file), cfg =>
            cfg
                .WithMaxTrials(3)
                .WithStrategy(DownloadStrategy.DownloadToSharepoint)
                .WithJob(context.Job)
                .WithTransmittal(context.TransmittalItem)
                .WithServiceProvide(context.ServiceProvider, false));

            result.CompletionTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {

                }
                else if (t.IsCanceled)
                {

                }
                else if (t.IsCompleted)
                {
                    context.State.SetFileState(file.Url, "Completed");
                }
            });
            return result.CompletionTask;
        }

        public async Task<IncommingTransmitalContext> DownloadFiles(IncommingTransmitalContext context)
        {
            var tasks = context.Transmittal.Documents
                .Select(f => StartDownload(f, context));

            await Task.WhenAll(tasks);
            return context;
        }
        public async Task<IncommingTransmitalContext> CancelDownloads(IncommingTransmitalContext context)
        {
            await Task.CompletedTask;
            if (!string.IsNullOrWhiteSpace(context.Job?.InternalId))
            {
                this.downloader.Cancel(x => x.Job?.InternalId == context?.Job.InternalId);
            }
            return context;
        }
        private void SetResult(Task<IncommingTransmitalContext> task, IncommingTransmitalContext context)
        {
            if (this.items.TryRemove(context.Id, out var _temp))
            {
                //_temp.Dispose();
            }
            if (task.IsFaulted)
            {
                if (context.IsRetryable(task.Exception.GetBaseException()))
                {
                    //this.logger.LogInformation(
                    //    $"Failed to process job on Transmital{context.Transmittal} with {context.Trial} trials. We will retry in few seconds.");
                    context.IncrementTrials();
                    this.Enqueue(context);
                    //Task.Delay(context.IncrementTrials(), context.CancellationToken)
                    //    .ContinueWith(x =>
                    //    {
                    //        if (!x.IsCanceled)
                    //        {
                    //            this.Enqueue(context);
                    //        }
                    //    });
                }
                else
                {
                    //context.SendLog(LogLevel.Error, "My Error");
                    context.SendLog(LogLevel.Error,
                        $"FAILURE. Transmittal {context.Transmittal}" +
                        $"An unrecoverable error occured while trying to process this job:'{context.Job}'" +
                        $"\r\n Error:'{task.Exception.GetBaseException()?.Message}'");
                    context.CompletionSource.SetException(task.Exception);
                    _ = CancelDownloads(context);
                    //_temp?.Dispose();


                }

            }
            else if (task.IsCanceled)
            {
                context.CompletionSource.SetCanceled();
                _ = CancelDownloads(context);
                //_temp?.Dispose();
            }
            else if (task.IsCompleted)
            {
                context.SendLog(LogLevel.Information,
                    $"Successfully Finished Job:'{context.Job}' On Transmitall '{context.Transmittal}'.");
                context.CompletionSource.SetResult(task.Result);
                // _temp?.Dispose();
            }

        }
        public class FeedBackResponse
        {
            public string responseCode { get; set; }
            public string responseDesc { get; set; }

            public int GetCode() => int.TryParse(responseCode, out var res) ? res : -1;

        }
        private async Task SendResultFeedBack(string TR_NO, string status_code, string message)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("TR_NO", TR_NO);
            client.DefaultRequestHeaders.Add("RESPONSE_CODE", status_code);
            client.DefaultRequestHeaders.Add("RESPONSE_DESC", message);
            client.DefaultRequestHeaders.Add("username", "moshanir");
            client.DefaultRequestHeaders.Add("password", "M1234567");

            var url = "https://mycart.mapnagroup.com/group_app/ws_dc/npx/getresult";
            try
            {
                var response = await client.PostAsync(url, new StringContent("Feedback"));
                response.EnsureSuccessStatusCode();
                var str = await response.Content.ReadAsStringAsync();
                var r = Newtonsoft.Json.JsonConvert.DeserializeObject<FeedBackResponse>(str);
                if (r.GetCode() != 0)
                {
                    throw new Exception($"Code:'{r.responseCode}'. Message :{r.responseDesc}");

                }

                this.logger.LogInformation(
                    $"Feedback Successfully Sent to '{url}'. TR_NO: '{TR_NO}', RESPONSE_CODE: '{status_code}', RESPONSE_DESC: '{message}'  ");

            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to send feedback on transmittal receive to url:{url}. Error:{err.GetBaseException().Message} ");
            }
        }
        private async Task<IncommingTransmitalContext> SetStatus(IncommingTransmitalContext context)
        {
            var task = context.CompletionTask;
            if (context.Job == null)
                return context;
            if (!task.IsCompleted)
                return context;
            if (task.IsCanceled)
            {
                //await context.GetRepository().SetJobStatus(context.Job.InternalId, "Canceled");
            }
            else if (task.IsFaulted)
            {
                await context.GetRepository().SetJobStatus(context.Job.InternalId, "Failed");
                await SendResultFeedBack(context.Transmittal.TR_NO, "-1", task.Exception.GetBaseException().Message);
            }
            else
            {
                await context.GetRepository().SetJobStatus(context.Job.InternalId, "Completed");
                await SendResultFeedBack(context.Transmittal.TR_NO, "0", "Success");

            }
            return context;
        }
        private async Task<IncommingTransmitalContext> Post(IncommingTransmitalContext context)
        {
            throw new TransmitalException("Some error", false);
            await context.GetRepository().SetJobStatus(context.Job.InternalId, "Completed");
            return context;
        }

        private async Task DoEnqueue(CancellationToken token, IncommingTransmitalContext context)
        {
            try
            {
                await WithAsync<IncommingTransmitalContext>
                    .Setup()
                    .First(ctx => ctx.Validate()
                        .WithAction(x => x.Get(constructor: sp => this.downloader))
                        .WithServiceProvide(this.serviceProvider)
                        .SetCannellationToken(token))
                    .Then(IncommingQueueSteps.CheckTrials)
                    .Then(IncommingQueueSteps.Validate)
                    .Then(IncommingQueueSteps.EnsureJob)
                    .Then(IncommingQueueSteps.GetOrAddTransmittal)
                    .Then(IncommingQueueSteps.DownloadLetter)
                    .Then(IncommingQueueSteps.DownloadFiles)
                    .Then(IncommingQueueSteps.UpdateTransmittalIssueState)
                    .Run(context)
                    .ContinueWith(t => SetResult(t, context));
                await CancelDownloads(context);
                await SetStatus(context);
                context.Dispose();
            }
            catch (Exception err)
            {
                try
                {
                    context.SendLog(LogLevel.Critical,
                        $"Unexpected. An unexpected error occured while tryning to process incomming transmital. Error:{err.Message}");
                }
                catch { }
            }

        }
        public IncommingTransmitalContext Enqueue(IncommingTransmitalContext result)
        {
            if (!this.items.TryAdd(result.Id, result) || !base.Enqueue(t => DoEnqueue(t, result)))
            {
                //throw new Exception("Failed to add to queue");
            }
            return result;
        }
        public IncommingTransmitalContext Enqueue(TransmittalSubmitModel transmital, Action<IncommingTransmitalContext> configure = null)
        {
            if (this.items.TryGetValue(transmital.GetInternalId(), out var result))
            {
                return result;
            }
            result = new IncommingTransmitalContext() { Transmittal = transmital };
            configure?.Invoke(result);
            return Enqueue(result);
            if (!this.items.TryAdd(result.Id, result) || !base.Enqueue(t => DoEnqueue(t, result)))
            {
                throw new Exception("Failed to add to queue");
            }
            return result;
        }
    }
}
