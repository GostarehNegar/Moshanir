using Mapna.Transmittals.Exchange.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mapna.Transmittals.Exchange.Services.Queues.Incomming.Steps
{
    internal static partial class IncommingQueueSteps
    {
        private static Task StartDownload(TransmittalFileSubmitModel file, IncommingTransmitalContext context)
        {
            var downloader = context.ServiceProvider.GetService<IFileDownloadQueue>();
            context.State.SetFileState(file.Url, "InProgress");
            //var fileName = context.GetDestinationFileName(file);
            var fileName = context.GetSharePointDestinationPath(file);
            var result = downloader.Enqueue(file.Url, context.GetSharePointDestinationPath(file), cfg =>
            cfg
                .WithMaxTrials(3)
                .WithStrategy(DownloadStrategy.DownloadToSharepoint)
                .WithJob(context.Job).WithTransmittal(context.TransmittalItem)
                .WithServiceProvide(context.ServiceProvider, false))
                .WithFile(file);

            result.CompletionTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {

                }
                else if (t.IsCanceled)
                {

                }
                else if (t.IsCompleted)
                {
                    context.State.SetFileState(file.Url, "Completed");
                }
            });
            return result.CompletionTask;
        }

        public static async Task<IncommingTransmitalContext> DownloadFiles(IncommingTransmitalContext context)
        {
            var tasks = context.Transmittal.Documents
                .Select(f => StartDownload(f, context));

            await Task.WhenAll(tasks);
            return context;
        }
        
        public static async Task<IncommingTransmitalContext> RemoveDuplicateFiles(IncommingTransmitalContext context)
        {
            var docs = (await context.GetRepository().GetDocumentsByTransmittal(context.TransmittalItem.TransmittalNo))
                .Where(x => x.FileSystemObjectType == Microsoft.SharePoint.Client.FileSystemObjectType.File)
                .ToArray();
                
            var trans_docs = context.Transmittal.Documents;
            var dupliactes = docs.Where(x =>!trans_docs.Any(d=>string.Compare(x.FileLeafRef,d.FileName,true)==0) ).ToArray();
            foreach (var doc in dupliactes)
            {
                //doc.FileSystemObjectType== Microsoft.SharePoint.Client.FileSystemObjectType.File
                await context.GetRepository().DeleteDocument(doc);
                context.SendLog(Microsoft.Extensions.Logging.LogLevel.Information,
                    $"File :{doc.FileLeafRef} deleted. Because it is detecte as a duplicate file!");

            }

            

            return context;
        }
        public static async Task<IncommingTransmitalContext> CancelDownloads(IncommingTransmitalContext context)
        {
            var downloader = context.Get<IFileDownloadQueue>();
            await Task.CompletedTask;
            if (!string.IsNullOrWhiteSpace(context.Job?.InternalId))
            {
                downloader.Cancel(x => x.Job?.InternalId == context?.Job.InternalId);
            }
            return context;
        }
    }
}
