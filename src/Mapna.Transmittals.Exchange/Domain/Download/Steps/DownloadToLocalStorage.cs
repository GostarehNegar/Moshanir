using Mapna.Transmittals.Exchange.Internals;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.Services.Queues.Download.Steps
{
    public static partial class DownloadSteps
    {
        public static async Task<FileDownloadContext> DownloadFileToLocalStorage(FileDownloadContext ctx)
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
                using (var file = new FileStream(ctx.Destination, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    //await response.Content.CopyToAsync(file);
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        await stream.CopyToAsync(file, 1024 * 4, ctx.CancellationToken);
                    }
                }
            }
            return ctx;
        }
        public static async Task<FileDownloadContext> DownloadFileWithProgress(FileDownloadContext ctx)
        {


            using (var client = new WebClient())
            {
                var done = false;
                var t = Task.Run(async () =>
                {
                    while (!done)
                    {
                        await Task.Delay(500);
                        if (ctx.CancellationToken.IsCancellationRequested)
                        {
                            client.CancelAsync();
                            ctx.CancellationToken.ThrowIfCancellationRequested();
                            break;
                        }
                    }
                });
                client.DownloadProgressChanged += (a, b) =>
                {
                    ctx.InvokeProgress(b.BytesReceived, b.TotalBytesToReceive);
                };
                await client.DownloadFileTaskAsync(ctx.Url, ctx.Destination);
                done = true;
                await t;
            }
            return ctx;
        }

    }
}
