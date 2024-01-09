using GN.Library.SharePoint.Internals;
using Mapna.Transmittals.Exchange.Internals;
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

            var md2 = await ctx.GetRepository().GetCompany("MOS");
            if (md2 == null)
            {
                throw new Exception(
                    $"Failed to find MOS in company list. We need this to set the 'To' feild on the received transmittal.");
            }
            var disc = await ctx.GetRepository().GetDiscipline("General");
            if (disc == null)
            {
                throw new Exception(
                    $"Failed to find General Disciplie. We need this to set the discipline feild on the received transmittal.");
            }
            SPTransmittalItem referto = null;
            if (string.Compare(transmittal.TR_ACTION, "ReplyIssue", true) == 0)
            {
                if (!string.IsNullOrWhiteSpace(ctx.Transmittal.ReferedTo))
                {
                    referto = await ctx.GetRepository().GetTransmittal(ctx.Transmittal.ReferedTo);
                    if (referto == null)
                    {
                        throw new Exception(
                            $"ReferedTo Transmittal Not Found: '{ctx.Transmittal.ReferedTo}'");
                    }
                }
            }


            var item = await ctx.GetRepository().GetOrAddTransmittal(ctx.Transmittal.TR_NO, t =>
            {
                t.ToLook = new Microsoft.SharePoint.Client.FieldLookupValue { LookupId = md2.Id };
                t.ReferenceNumber = transmittal.TR_NO;
                t.Title = transmittal.Title;
                if (t.Title!=null && t.Title.Length > 100)
                {
                    t.Title = t.Title.Substring(0, 98);
                }
                t.TrAction = transmittal.TR_ACTION;// "FirstIssue";
                t.DiscFirstLook0 = new Microsoft.SharePoint.Client.FieldLookupValue { LookupId = disc.Id };
                if (referto != null)
                {
                    t.SetAttributeValue("PurseToLook", new Microsoft.SharePoint.Client.FieldLookupValue { LookupId = referto.Id });
                }
            }, transmittal.TR_ACTION);

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
            await Task.Delay(10000, ctx.CancellationToken);
            item = await ctx
                    .GetRepository()
                    .GetTransmittalById(item.Id);
            // Maybe the cause for "Version Conflict" issue.
            item = await ctx.GetRepository().GetOrAddTransmittal(ctx.Transmittal.TR_NO, t =>
            {
                t.TrAction = transmittal.TR_ACTION;// "FirstIssue";
                if (referto != null)
                {
                    t.SetAttributeValue("PurseToLook", new Microsoft.SharePoint.Client.FieldLookupValue { LookupId = referto.Id });
                }

            });
            ctx.TransmittalItem = item;


            return ctx;
        }

        public static async Task<IncommingTransmitalContext> UpdateReferTo(IncommingTransmitalContext ctx)
        {
            var refer = ctx.Transmittal.ReferedTo;
            var no = ctx.Transmittal.TR_NO;
            if (!string.IsNullOrWhiteSpace(refer))
            {
                var referto = ctx.GetRepository().GetTransmittal(refer);
                if (referto == null)
                {
                    throw new Exception(
                        $"ReferedTo Transmittal Not Found: '{refer}'");
                }
                await ctx.GetRepository().GetOrAddTransmittal(ctx.Transmittal.TR_NO, t =>
                {


                });
            }



            return ctx;
        }
    }
}
