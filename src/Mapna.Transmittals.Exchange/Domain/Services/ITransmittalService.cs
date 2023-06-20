using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Mapna.Transmittals.Exchange.Internals;
using Mapna.Transmittals.Exchange.Services.Queues.Incomming;
using Mapna.Transmittals.Exchange.Services.Queues;

namespace Mapna.Transmittals.Exchange.Services
{
    public interface ITransmittalService
    {
        Task<SubmitTransmittalReply> Submit(TransmittalSubmitModel transmittal);
    }
    class TransmitallService : ITransmittalService
    {
        private readonly ILogger<TransmitallService> logger;
        private readonly ITransmittalRepository repository;
        private readonly IIncommingQueue incomming;

        public TransmitallService(ILogger<TransmitallService> logger, ITransmittalRepository repository, IIncommingQueue incomming)
        {
            this.logger = logger;
            this.repository = repository;
            this.incomming = incomming;
        }
        public async Task<TransmittalProcessingContext> TryValidate(TransmittalProcessingContext ctx)
        {
            Exception err = null;
            var transmittal = ctx.Transmittal;
            foreach (var file in transmittal.Documents ?? new TransmittalFileSubmitModel[] { })
            {
                var file_in_master_list = await this.repository.FindInMasterList(file.DocNumber);
                if (file_in_master_list == null)
                {
                    err = new Exception(
                        $"Invalid Transmittal File. '{file}' is missing in target master list.");
                }
            }
            if (err != null)
            {
                throw err;
            }
            return ctx;
        }
        public async Task<TransmittalProcessingContext> ValidateTransmittal(TransmittalProcessingContext context)
        {
            if (!context.Transmittal.TryValidate(out var exp))
            {
                throw exp;
            }
            return context;
        }
        public async Task<TransmittalProcessingContext> ValidateJob(TransmittalProcessingContext context)
        {
            var job = await this.repository.FindJob(context.Transmittal.GetInternalId());
            if (job != null && (job.Status == SPJobItem.Schema.Statuses.Completed))
            {
                throw new TransmitalException(
                    $"Transmittal '{context.Transmittal}' is already processed with Job:{job}");
            }
            return context;
        }
        public async Task<SubmitTransmittalReply> Submit(TransmittalSubmitModel transmittal)
        {
            // The underlying connection was closed
            var result = new SubmitTransmittalReply();
            result.Failed = 1;
            var trials = 0;
            while (trials < 5)
            {
                try
                {
                    await GN.Library.Helpers.WithAsync<TransmittalProcessingContext>.Setup()
                        .Then(ValidateTransmittal)
                        .Then(TryValidate)
                        .Then(ValidateJob)
                        .Run(new TransmittalProcessingContext { Transmittal = transmittal });
                    this.incomming.Enqueue(transmittal, cfg => { });
                    result.Failed = 0;
                    return result;
                }
                catch (Exception err)
                {
                    if (err.GetBaseException()?.Message == null || !err.GetBaseException().Message.Contains("underlying connection was closed"))
                    {
                        throw;
                    }
                }
                await Task.Delay(2000);

            }
            return result;


        }
    }

}
