using Mapna.Transmittals.Exchange.Services.Queues;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.Services.Queues.Incomming.Steps
{
    internal static partial class IncommingQueueSteps
    {
        public static async Task<IncommingTransmitalContext> UpdateTransmittalIssueState(IncommingTransmitalContext context)
        {
            await context.GetRepository()
                .SetTransmittalIssueState(context.TransmittalItem.TransmittalNo, Internals.SPTransmittalItem.Schema.IssueStates.Accept);
            return context;
        }
    }
}
