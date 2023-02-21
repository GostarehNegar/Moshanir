using Mapna.Transmittals.Exchange.Internals;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.Services.Queues.Incomming.Steps
{
    internal static partial class IncommingQueueSteps
    {
        public static async Task<IncommingTransmitalContext> EnsureJob(IncommingTransmitalContext context)
        {
            var job = await context.GetRepository()
                .FindJobByInternalId(context.Id);
            if (job == null)
            {
                job = await context.GetRepository().CreateJob(new SPJobItem
                {
                    Content = MapnaTransmittalsExtensions.Serialize(context.Transmittal),
                    InternalId = context.Id,
                    SourceId = context.Transmittal.GetInternalId(),
                    Title = context.Title,
                    Direction = "In",
                });
            }
            else if (job.Status == SPJobItem.Schema.Statuses.Completed)
            {
                throw new TransmitalException(
                    $"Transmittal '{context.Transmittal}' has been already received in Job:{job}.", false);
            }
            else
            {
                job.Content = MapnaTransmittalsExtensions.Serialize(context.Transmittal);
                job.InternalId = context.Id;
                job.Status = SPJobItem.Schema.Statuses.InProgress;
                await context.GetRepository().UpdateJob(job);
            }
            context.Job = job;
            context.SendLog(LogLevel.Information, $"Starting Job '{job}' On Transmital '{context.Transmittal}'");
            return context;
        }
    }
}
