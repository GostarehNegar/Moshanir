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
                var fileName = context.GetDestinationFileName(new TransmittalFileSubmitModel { FileName = $"{context.Transmittal.TR_NO}-Letter.zip" });
                var attachments = (await SPListExtensions.GetAttachments(context.TransmittalItem));
                var attachment = (await SPListExtensions.GetAttachments(context.TransmittalItem))
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
                for (var i = 0; i < 3; i++) {
                    try
                    {
                        // Try Setting Letter Field
                        attachment = (await SPListExtensions.GetAttachments(context.TransmittalItem))
                           .FirstOrDefault(x => x.Name == Path.GetFileName(fileName));
                        //var att = context.TransmittalItem.GetAttachments().FirstOrDefault();

                        context.TransmittalItem = await repo.GetTransmittal(context.TransmittalItem.TransmittalNo);
                        if (attachment != null)
                        {
                            context.TransmittalItem.SetAttributeValue("Letter", new Microsoft.SharePoint.Client.FieldUrlValue
                            {
                                Url = repo.ToAbsoultePath(attachment.ServerRelativeUrl),
                                Description = "Scan"
                            });
                            await repo.UpdateTransmittal(context.TransmittalItem);
                        }
                        break;
                    }
                    catch (Exception err1)
                    {
                        context.SendLog(LogLevel.Warning,
                            $"An error occured while trying to set Letter Url. Err:{err1.Message}");
                        await Task.Delay(5000);
                    }
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
