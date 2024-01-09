using Mapna.Transmittals.Exchange.Internals;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.Services.Queues.Download.Steps
{
    public static partial class DownloadSteps
    {
        public static Task<FileDownloadContext> DownloadFile(FileDownloadContext ctx)
        {
            switch (ctx.Stratgey)
            {
                case DownloadStrategy.DownloadToLocal:
                    return DownloadFileToLocalStorage(ctx);
                default:
                    return DownloadFileToSharePointTemp(ctx);
            }
            //return DownloadFileToLocalStorage(ctx);
        }
        public static async Task<FileDownloadContext> DownloadFileToSharePointTemp(FileDownloadContext ctx)
        {
            //user: service_user
            //pass:edms#1401
            

            using (var client = new HttpClient())
            {
                ctx.SendLog(LogLevel.Information, $"Downloading Starts Url: '{ctx.Url}', Location:'{ctx.Destination}', Trials:{ctx.Trial}");
                // Basic authorization
                // Seems that urls do not need authorization.
                //client.DefaultRequestHeaders.Authorization =
                //    new AuthenticationHeaderValue( "Basic", Convert.ToBase64String(
                //            Encoding.ASCII.GetBytes($"service_user:edms#1401")));
                var response = await client.GetAsync(ctx.Url);
                response.EnsureSuccessStatusCode();
                /// We no longer require to create this directory
                /// 
                //if (!Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(ctx.Destination))))
                //{
                //    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(ctx.Destination)));
                //}
                //if (File.Exists(ctx.Destination))
                //{
                //    File.Delete(ctx.Destination);
                //}
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var relativePath = "";
                    var fileName = ctx.Destination;
                    var idx = fileName.LastIndexOf('/');
                    if (idx > -1)
                    {
                        relativePath = fileName.Substring(0, idx + 1);
                        fileName = fileName.Substring(idx).Replace("/", "");
                    }
                    if (relativePath.Length > 0 && !relativePath.StartsWith("/"))
                    {
                        relativePath = "/" + relativePath;
                    }
                    if (1 == 1)
                    {
                        ctx.Destination = await ctx.GetRepository().UploadDoc(relativePath, fileName, stream, item =>
                        {
                            if (ctx.FileSubmitModel != null)
                            {
                                item.Status = ctx.FileSubmitModel.Status;
                                item.ExtRev = ctx.FileSubmitModel.Ext_Rev;
                                item.IntRev = ctx.FileSubmitModel.Int_Rev;
                                item.Purpose = ctx.FileSubmitModel.Purpose;
                            }

                        });
                    }
                    else
                    {
                        ctx.Destination = await ctx.GetRepository().UploadDoc(relativePath, fileName, stream);
                    }
                    var file = await ctx.GetRepository().GetDocumentByPath(ctx.Destination);
                    if (file == null)
                    {
                        throw new Exception($"Failed to upload file. Cannot get the file with the anticipated Path.");
                    }
                    if (file.IntRev!= ctx.FileSubmitModel.Int_Rev || file.ExtRev!= ctx.FileSubmitModel.Ext_Rev || file.Status!= ctx.FileSubmitModel.Status)
                    {
                        file.IntRev = ctx.FileSubmitModel.Int_Rev;
                        file.ExtRev = ctx.FileSubmitModel.Ext_Rev;
                        file.Status = ctx.FileSubmitModel.Status;
                        file.Purpose = ctx.FileSubmitModel.Purpose;
                        await ctx.GetRepository().UpdateDocument(file);
                        
                    }
                    ///
                    /// just make sure item metadata is okat
                    ///

                }

                //using (var file = new FileStream(ctx.Destination, FileMode.OpenOrCreate, FileAccess.Write))
                //{
                //    //await response.Content.CopyToAsync(file);

                //}
            }
            return ctx;
        }
        public static async Task<FileDownloadContext> CheckFileMetadata(FileDownloadContext ctx)
        {
            await Task.Delay(1000);
            if (ctx.FileSubmitModel==null ||  ctx.Stratgey == DownloadStrategy.DownloadToLocal)
                return ctx;
            var file = await ctx.GetRepository().GetDocumentByPath(ctx.Destination);
            if (string.IsNullOrWhiteSpace(file.DocumentNumber))
            {

                // set_doc worflow has failed
                // we delete the document and hope that it would be fixed in a retry
                await ctx.GetRepository().DeleteDocument(file);
                throw new Exception(
                    $"Document Metadata was not correctly set for Documnet {ctx.FileSubmitModel.FileName}."+
                    " We will delete this document and hopw that this will be corrected in anothr trial.");




            }
            return ctx;

        }

    }
}
