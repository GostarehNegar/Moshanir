using Mapna.Transmittals.Exchange.GhodsNiroo.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.GhodsNiroo.Incoming.Steps
{
    internal static partial class IncomingSteps
    {
        public static async Task<IncomingTransmittalContext> Validate(IncomingTransmittalContext context)
        {


            return context;
        }
        public static async Task<IncomingTransmittalContext> EnsureJob(IncomingTransmittalContext context)
        {

            context.CancellationToken.ThrowIfCancellationRequested();
            var job = await context.GetSPContext().FindJobByInternalId(context.Id);
            if (job == null)
            {
                job = await (context.GetSPContext()).CreateJob(new SPJobItem
                {
                    Content = context.Serialize(context.Request),
                    InternalId = context.Id,
                    SourceId = context.Request.TR_NO,
                    Status = SPJobItem.Schema.Statuses.InProgress,
                    Title = context.Title,
                    Direction = "In",
                }); ;
            }
            else if (1 == 0 && job.Status == SPJobItem.Schema.Statuses.Completed)
            {
                throw new IncomingException(
                    $"Transmittal '{context.Id}' has been already received in Job:{job}.", false);
            }
            else
            {
                job.Content = context.Serialize(context.Request);
                job.InternalId = context.Id;
                job.Status = SPJobItem.Schema.Statuses.InProgress;
                await context.GetSPContext().UpdateJob(job);
            }
            context.Log(Microsoft.Extensions.Logging.LogLevel.Information,
                $"Job: '{job.Title}' Starts.");

            return context;

        }

        public static async Task<IncomingTransmittalContext> DownloadLetter(IncomingTransmittalContext context)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    context.Log(Microsoft.Extensions.Logging.LogLevel.Debug,
                        $"Transmittal Letter Download Starts. File: '{context.Request.Tr_file_Name}', Url:'{context.Request.Url}'");
                    var response = await client.GetAsync(context.Request.Url);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsByteArrayAsync();

                    await context.GetSPContext().UploadTransmittal(context.Request.Tr_file_Name, content, context.Request.TR_NO, context.Request.Project_Name, item =>
                     {
                     });
                    context.Log(Microsoft.Extensions.Logging.LogLevel.Information,
                        $"Transmittal Letter File Successfully Downloaded.");
                }
                catch (Exception err)
                {
                    throw;
                }
            }
            return context;
        }


        private static async Task<IncomingTransmittalContext> DownloadFile(IncomingTransmittalContext context, IncomingTransmittalRequest.FileModel file)
        {
            using (var client = new HttpClient())
            {
                context.Log(Microsoft.Extensions.Logging.LogLevel.Debug,
                    $"Download Starts. File: '{file}'");
                var response = await client.GetAsync(file.Url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsByteArrayAsync();

                await context.GetSPContext().UploadDocument(file.FileName, content, item =>
                {
                    item.Revision = file.Int_Rev;
                    item.Purpose = file.Purpose;
                    item.Inhouse = file.Ext_Rev;
                    item.Status = file.Status;

                });
                context.Log(Microsoft.Extensions.Logging.LogLevel.Information,
                    $"File '{file}' Successfully Downloaded.");
            }
            return context;
        }
        public static async Task<IncomingTransmittalContext> DownloadFiles(IncomingTransmittalContext context)
        {
            context.Request.Files = context.Request.Files ?? new List<IncomingTransmittalRequest.FileModel>();
            foreach (var file in context.Request.Files)
            {
                try
                {
                    await DownloadFile(context, file);
                }
                catch (Exception err)
                {
                    context.Log(Microsoft.Extensions.Logging.LogLevel.Error,
                        $"An error occured while trying to upload this file '{file}'. Error:{err.GetBaseException().Message}");
                    throw new Exception($"An error occured while trying to upload this file '{file}'. Error:{err.GetBaseException().Message}", err);
                }

            }
            return context;
        }
        public static async Task<IncomingTransmittalContext> SendResultFeedBack(IncomingTransmittalContext context)
        {



            return context;
        }


    }
}
