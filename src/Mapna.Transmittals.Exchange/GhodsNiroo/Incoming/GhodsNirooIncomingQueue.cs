using GN.Library.Helpers;
using GN.Library.TaskScheduling;
using Mapna.Transmittals.Exchange.GhodsNiroo.Incoming;
using Mapna.Transmittals.Exchange.GhodsNiroo.Incoming.Steps;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        private readonly ILogger<GhodsNirooIncomingQueue> logger;
        private readonly GhodsNirooTransmittalOptions options;
        private ConcurrentDictionary<string, IncomingTransmittalRequest> items = new ConcurrentDictionary<string, IncomingTransmittalRequest>();

        public GhodsNirooIncomingQueue(IServiceProvider serviceProvider, ILogger<GhodsNirooIncomingQueue> logger, GhodsNirooTransmittalOptions options) : base(1)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.options = options;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }
        private async Task SendResultFeedBack(string TR_NO, string status_code, string message)
        {
            using (var client = new HttpClient())
            {
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
                    //var str = await response.Content.ReadAsStringAsync();
                    //var r = Newtonsoft.Json.JsonConvert.DeserializeObject<FeedBackResponse>(str);
                    //if (r.GetCode() != 0)
                    //{
                    //    throw new Exception($"Code:'{r.responseCode}'. Message :{r.responseDesc}");

                    //}

                    this.logger.LogInformation(
                        $"Feedback Successfully Sent to '{url}'. TR_NO: '{TR_NO}', RESPONSE_CODE: '{status_code}', RESPONSE_DESC: '{message}'  ");

                }
                catch (Exception err)
                {
                    this.logger.LogError(
                        $"An error occured while trying to send feedback on transmittal receive to url:{url}. Error:{err.GetBaseException().Message} ");
                }
            }
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
                    await SendResultFeedBack(context.Request.TR_NO, "-1", "Canceled");
                }
                else if (task.IsFaulted)
                {
                    await context.GetSPContext().SetJobFailed(context.Id, task.Exception.GetBaseException().Message);
                    await SendResultFeedBack(context.Request.TR_NO, "-1", task.Exception.GetBaseException().Message);
                }
                else if (task.IsCompleted)
                {
                    await context.GetSPContext().SetJobCompleted(context.Id, "");
                    await SendResultFeedBack(context.Request?.TR_NO, "0", "Success");
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
