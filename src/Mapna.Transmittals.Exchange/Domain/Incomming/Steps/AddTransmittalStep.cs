using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.Services.Queues.Incomming.Steps
{
    internal static partial class IncommingQueueSteps
    {
        public static async Task<IncommingTransmitalContext> GetOrAddTransmittal(IncommingTransmitalContext ctx)
        {
            var transmittal = ctx.Transmittal;
            var count = await ctx.GetRepository().CountTransmitalls();
            var item = await ctx.GetRepository().GetOrAddTransmittal(ctx.Transmittal.TR_NO, t =>
            {
                t.ToLook = new Microsoft.SharePoint.Client.FieldLookupValue { LookupId = 56 };
                t.ReferenceNumber = transmittal.TR_NO;
                t.Title = transmittal.Title;
                t.TrAction = "FirstIssue";
                t.DiscFirstLook0 = new Microsoft.SharePoint.Client.FieldLookupValue { LookupId = 39 };
            });

            /// We would give some time to server
            /// to comeup with a transmittal number.
            /// 
            for (var trial = 0; trial < 10 && string.IsNullOrWhiteSpace(item.TransmittalNo); trial++)
            {
                if (ctx.CancellationToken.IsCancellationRequested)
                    break;
                item = await ctx
                    .GetRepository()
                    .GetTransmittalById(item.Id);
                await Task.Delay(2000, ctx.CancellationToken);
            }
            if (string.IsNullOrWhiteSpace(item.TransmittalNo))
            {
                throw new ValidationException(
                    $"Server didn't assign a valid 'Transmittal Numbmer'. Transmittal:{ctx.Transmittal} ");
            }
            ctx.TransmittalItem = item;


            return ctx;
        }
    }
}
