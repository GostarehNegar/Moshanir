using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.Services.Queues.Incomming.Steps
{
    internal static partial class IncommingQueueSteps
    {
        public static async Task<IncommingTransmitalContext> CheckTrials(IncommingTransmitalContext ctx)
        {
            await Task.CompletedTask;
            if (ctx.Trial > ctx.MaxTrials)
            {
                throw new TransmitalException(
                    $"Trial {ctx.Trial} exceeds maximum {ctx.MaxTrials}. For Transmitall{ctx.Transmittal} ", false);
            }
            var _wait = ctx.Trial * ctx.Trial * 6 * 1000;
            ctx.GetLogger().LogInformation(
                $"This is trials {ctx.Trial} of {ctx.MaxTrials}. We will wait {_wait} seconds.");
            await Task.Delay(ctx.Trial * ctx.Trial * 6 * 1000, ctx.CancellationToken);


            return ctx;
        }
        public static async Task<IncommingTransmitalContext> Validate(IncommingTransmitalContext ctx)
        {
            Exception err = null;
            var transmittal = ctx.Transmittal;
            foreach (var file in transmittal.Documents ?? new TransmittalFileSubmitModel[] { })
            {
                ctx.GetLogger().LogInformation($"Validating Document: '{file.DocNumber}'");
                var file_in_master_list = await ctx.GetRepository().FindInMasterList(file.DocNumber);
                if (file_in_master_list == null)
                {
                    err = new ValidationException(
                        $"Invalid Transmittal File. '{file}' is missing in target master list.", false);
                }
            }
            if (err != null)
            {
                throw err;
            }

            return ctx;
        }
    }
}
