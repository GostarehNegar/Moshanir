using GN.Library.Functional;
using GN.Library.Helpers;
using GN.Library.TaskScheduling;
using Mapna.Transmittals.Exchange.Domain.Outgoing.Steps;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mapna.Transmittals.Exchange.Internals;

namespace Mapna.Transmittals.Exchange.Domain.Outgoing
{
    public interface IOutgoingQueue
    {
        TransmittalOutgiongContext Enqueue(string transmittal, Action<TransmittalOutgiongContext> configure = null);
    }
    class TransmittalOutgoingQueue : BackgroundMultiBlockingTaskHostedService, IOutgoingQueue
    {
        private readonly ILogger<TransmittalOutgoingQueue> logger;
        private readonly IServiceProvider serviceProvider;
        private ConcurrentDictionary<string, TransmittalOutgiongContext> items = new ConcurrentDictionary<string, TransmittalOutgiongContext>();

        public TransmittalOutgoingQueue(ILogger<TransmittalOutgoingQueue> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }
        public TransmittalOutgiongContext Enqueue(string transmittal, Action<TransmittalOutgiongContext> configure = null)
        {
            if (items.TryGetValue(transmittal, out var result))
            {
                return result;
            }
            result = new TransmittalOutgiongContext()
            {
                TransimttalNumber = transmittal,
                Scope = this.serviceProvider.CreateScope()
            };
            configure?.Invoke(result);
            if (!items.TryAdd(transmittal, result) || !base.Enqueue(async t =>
            {
                var options = result.ServiceProvider.GetRequiredService<TransmittalsExchangeOptions>();
                try
                {
                    result.CancellationToken = t;
                    await WithPipe<TransmittalOutgiongContext>
                       .Setup(pipe => { result.Pipe = pipe; })
                       .Retrials(options.MaxTrialsInSendinfTransmittals)
                       .Then(OutgoingSteps.Load)
                       .Then(OutgoingSteps.GetOrCreateJob)
                       .Then(OutgoingSteps.Send)
                       .Then(OutgoingSteps.UpdateJob)
                       .Run(result);
                }
                catch (Exception err)
                {
                    //this.logger.LogError(
                    //    $"An error occured while trying to process outgoing transmittal. Error:{err.GetBaseException().Message}");
                    result.SendLog(LogLevel.Error,
                        $"An error occured while trying to process outgoing transmittal '{transmittal}'. Error:{err.GetBaseException().Message}");
                    try
                    {
                        await result.GetRepository().SetJobStatus(result.TransimttalNumber, SPJobItem.Schema.Statuses.Failed, err.Message);
                    }
                    catch { }

                }
                result.Dispose();

            }))
            {
                throw new Exception("Failed to enqueue item.");
            }
            return result;

        }
    }
}
