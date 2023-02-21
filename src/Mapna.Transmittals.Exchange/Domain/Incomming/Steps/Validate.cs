﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.Services.Queues.Incomming.Steps
{
    internal static partial class IncommingQueueSteps
    {
        public static async Task<IncommingTransmitalContext> Validate(IncommingTransmitalContext ctx)
        {
            Exception err = null;
            var transmittal = ctx.Transmittal;
            foreach (var file in transmittal.Documents ?? new TransmittalFileSubmitModel[] { })
            {
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