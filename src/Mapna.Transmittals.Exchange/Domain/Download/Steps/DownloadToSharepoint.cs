using Mapna.Transmittals.Exchange.Internals;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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
            using (var client = new HttpClient())
            {
                ctx.SendLog(LogLevel.Information, $"Downloading Starts Url: '{ctx.Url}', Location:'{ctx.Destination}', Trials:{ctx.Trial}");
                var response = await client.GetAsync(ctx.Url);
                response.EnsureSuccessStatusCode();
                if (!Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(ctx.Destination))))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(ctx.Destination)));
                }
                if (File.Exists(ctx.Destination))
                {
                    File.Delete(ctx.Destination);
                }
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
                    ctx.Destination = await ctx.GetRepository().UploadDoc(relativePath, fileName, stream);
                    //var file = await ctx.GetRepository().GetDocumentByPath(ctx.Destination);
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

    }
}
