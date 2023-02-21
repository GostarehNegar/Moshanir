using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Mapna.Transmittals.Exchange.Internals;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using GN.Library.SharePoint;
using System.Linq;

namespace Mapna.Transmittals.Exchange.Services.Queues.Incomming.Steps
{
    internal static partial class IncommingQueueSteps
    {
        public static async Task<IncommingTransmitalContext> DownloadLetter(IncommingTransmitalContext context)
        {
            try
            {
                var url = context.Transmittal.Url;
                var service = context.ServiceProvider.GetService<IFileDownloadQueue>();
                var repo = context.ServiceProvider.GetService<ITransmittalRepository>();
                var fileName = context.GetDestinationFileName(new TransmittalFileSubmitModel { FileName = $"{context.Transmittal.TR_NO}-Letter.pdf" });
                var attachments = (await context.TransmittalItem.GetAttachments());
                var attachment = (await context.TransmittalItem.GetAttachments())
                        .FirstOrDefault(x => x.Name == Path.GetFileName(fileName));
                /// We allow only one attachment
                /// 
                if (attachments.Length == 0)// attachment == null)
                {
                    context.SendLog(LogLevel.Information,
                        $"Start Receiving Transmittal Letter. Url:'{url}', Destination :{fileName}.");
                    var ctx = service.Enqueue(url, fileName, cfg =>
                    {
                        cfg.Stratgey = DownloadStrategy.DownloadToLocal;
                        cfg.WithJob(context.Job).WithMaxTrials(3);
                    });
                    await ctx.CompletionTask;
                    if (!File.Exists(ctx.Destination))
                    {
                        throw new Exception(
                            $"Unexpected Error. File:'{ctx.Destination}' not found.");
                    }
                    var content = File.ReadAllBytes(ctx.Destination);
                    await repo.AttachTransmittalLetter(context.TransmittalItem.Id, Path.GetFileName(ctx.Destination), content);
                }
                context.SendLog(LogLevel.Information,
                   $"Transmittal Letter Successfully Attached. Transmittal:{context.Transmittal}");
            }
            catch (Exception err)
            {
                context.SendLog(LogLevel.Error,
                    $"An error occured while trying to download transmittal letter. The job will Fail. ");
                throw new TransmitalException(
                    $"Failed to download the transmittall letter. The original Error:{err.GetBaseException().Message}");
            }
            return context;
        }
    }
}
