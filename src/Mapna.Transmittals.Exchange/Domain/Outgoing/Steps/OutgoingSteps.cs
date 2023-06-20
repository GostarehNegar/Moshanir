using GN.Library.Functional;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Mapna.Transmittals.Exchange.Models;
using Microsoft.Extensions.Logging;
using Mapna.Transmittals.Exchange.Internals;
using GN.Library.SharePoint;
using System.Net.Http;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Mapna.Transmittals.Exchange.Domain.Outgoing.Steps
{
    class OutgoingSteps
    {
        public static async Task Load(TransmittalOutgiongContext ctx, IWithPipe pipe, Func<TransmittalOutgiongContext, Task> n)
        {
            ctx.CancellationToken.ThrowIfCancellationRequested();

            ctx.SPTransmittalItem = await ctx.GetRepository().GetTransmittal(ctx.TransimttalNumber);
            if (ctx.SPTransmittalItem == null)
            {
                throw new PipeUnrecoverableException($"Transmittal Not Found: {ctx.TransimttalNumber}");
            }
            var letterFile = ctx.SPTransmittalItem.GetAttachments().FirstOrDefault();

            ctx.Transmittal = new TransmittalOutgoingModel
            {
                TransmitallNumber = ctx.SPTransmittalItem.TransmittalNo,
                TransmittalTitle = ctx.SPTransmittalItem.TransmittalTitle,
                Url = ctx.SPTransmittalItem.GetAttachments().FirstOrDefault()?.GetDownloadableUrl(),
                LetterFileName = letterFile?.Name
            };
            var files = await ctx.GetRepository().GetDocumentsByTransmittal(ctx.TransimttalNumber);
            //var fff = files[1].GetAttibuteValue<FieldLookupValue>("DocNoLook");

            ctx.Transmittal.Files = files
                .Where(x => x.FileSystemObjectType == Microsoft.SharePoint.Client.FileSystemObjectType.File)
                .Select(x => new TransmittalOutgoingFileModel
                {
                    ServerRelativePath = x.GetAttibuteValue<string>("FileRef"),
                    DocumentNumber = x.DocumentNumber,
                    Purpose = "FC",// x.Purpose,
                    Staus = "Approved",// "C1",// x.Status,
                    ExtRev = x.ExtRev,
                    IntRev = x.IntRev
                }).ToArray();

            ctx.GetLogger().LogInformation(
                $"Trannsmittal '{ctx.Transmittal.TransmitallNumber}' Successfully Loaded. ");

            await n(ctx);

        }
        public static async Task<TransmittalOutgiongContext> GetOrCreateJob(TransmittalOutgiongContext ctx)
        {
            var job = await ctx.GetRepository().FindJobByInternalId(ctx.TransimttalNumber);
            if (job == null)
            {
                job = await ctx.GetRepository().CreateJob(new SPJobItem
                {
                    Content = MapnaTransmittalsExtensions.Serialize(ctx.Transmittal),
                    InternalId = ctx.Transmittal.TransmitallNumber,
                    //SourceId = context.Transmittal.GetInternalId(),
                    Title = ctx.Transmittal.TransmittalTitle,
                    Direction = "Out",
                });

            }
            else if (job.Status == SPJobItem.Schema.Statuses.Completed)
            {
                throw new TransmitalException(
                    $"Transmittal '{ctx.TransimttalNumber}' has been already received in Job:{job}.", false);
            }
            else
            {
                await ctx.GetRepository().SetJobStatus(job.InternalId, SPJobItem.Schema.Statuses.InProgress);
            }
            return ctx;
        }
        public static async Task<TransmittalOutgiongContext> Send(TransmittalOutgiongContext ctx)
        {

            using (var client = new HttpClient())
            {
                var url = ctx.ServiceProvider.GetService<TransmittalsExchangeOptions>().MapnaEndpoint;
                client.DefaultRequestHeaders.TryAddWithoutValidation("TR_NO", ctx.TransimttalNumber);
                client.DefaultRequestHeaders.TryAddWithoutValidation("username", "moshanir");
                client.DefaultRequestHeaders.TryAddWithoutValidation("password", "M1234567");
                var body = MapnaTransmittalsExtensions.Serialize(ctx.Transmittal.ToSubmitModel());
                var response = await client.PostAsync(url, new StringContent(body));
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                if (MapnaTransmittalsExtensions.TryDeserialize<MapnaResponseModel>(responseBody, out var mapnaResponse, true) &&
                    int.TryParse(mapnaResponse.ResponseCode, out var errorcode))
                {
                    if (errorcode != 0)
                    {
                        throw new Exception(
                            $"Failed to send Transmittal. Mapna Endpoint Retuened: ErrorCode:'{errorcode}', ErrorMessage:'{mapnaResponse.ResponseDesc}'");
                    }
                }
                else
                {
                    throw new Exception(
                        $"Invalid Response from Mapna Endpoint. Response Body:'{responseBody}'");
                }
                ctx.SendLog(LogLevel.Information,
                    $"Transmittal '{ctx.TransimttalNumber}' successfully sent to Mapna Endpoint. ResponseCode:{mapnaResponse.ResponseCode}");

            }
            return ctx;
        }
        public static async Task<TransmittalOutgiongContext> UpdateJob(TransmittalOutgiongContext ctx)
        {
            await ctx.GetRepository().SetJobStatus(ctx.TransimttalNumber, SPJobItem.Schema.Statuses.Waiting);
            ctx.SendLog(LogLevel.Information,
                $"Job Status for Transmittal '{ctx.TransimttalNumber}' changed to 'Waiting'. We will wait for the feedback result.");
            return ctx;
        }


    }
}
