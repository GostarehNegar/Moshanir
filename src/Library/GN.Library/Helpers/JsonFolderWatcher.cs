using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Helpers
{

    public delegate Task FileChangeEvent<T>(IJsonFolderWatcher<T> sender, string fileName, WatcherChangeTypes? changeType, T value);
    public delegate Task FileCreateEvent<T>(IJsonFolderWatcher<T> sender, T value);
    public interface IJsonFolderWatcher<T> : IDisposable
    {
        Task StartAsync(CancellationToken cancellation);
        event FileChangeEvent<T> OnChange;
    }
    public class JsonFolderWatcher<T> : IJsonFolderWatcher<T>
    {
        public static ILogger logger = typeof(JsonFolderWatcher<T>).GetLoggerEx();
        public string Folder { get; private set; }
        public string Pattern { get; set; }
        private FileSystemWatcher watcher;
        private CancellationToken cancellationToken;
        private Encoding encoding;

        public event FileChangeEvent<T> OnChange;

        public JsonFolderWatcher(string folder, string pattern, Encoding encoding = null)
        {
            this.Folder = folder;
            this.Pattern = pattern;
            this.cancellationToken = default(CancellationToken);
            this.encoding = encoding;

        }
        private Task<bool> HandleFile(string fileName, WatcherChangeTypes? changeType)
        {
            return Task.Run<bool>(async () =>
            {
                bool result = false;
                try
                {
                    var message = await UtilityHelpers.CreateTaskWithTimeOut<T>(
                        work: () =>
                        {
                            T __result = default(T);

                            if (File.Exists(fileName))
                            {
                                var txt = this.encoding == null
                                    ? File.ReadAllText(fileName)
                                    : File.ReadAllText(fileName, this.encoding);
                                if (!string.IsNullOrWhiteSpace(txt))
                                {
                                    __result = AppHost.Utils.Deserialize<T>(txt);
                                }
                            }
                            return __result;
                        },
                        token: this.cancellationToken,
                        timeOut: 10 * 1000,
                        Throw: false)
                        .ConfigureAwait(false);
                    result = message != null;
                    if (!this.cancellationToken.IsCancellationRequested)
                    {
                        this.OnChange?.Invoke(this, fileName, changeType, message);
                    }
                    return true;
                }
                catch (Exception err)
                {
                    if (!(err is TimeoutException && cancellationToken.IsCancellationRequested))
                    {
                        logger.LogError(
                            $"An error occured while trying to process json file. File:{fileName}, Error:{err.Message}");
                    }
                    else
                    {
                        throw;
                    }
                    result = false;
                }
                return result;
            });
        }

        public async Task StartAsync(CancellationToken cancellation)
        {
            try
            {
                this.cancellationToken = cancellation;
                var existingFiles = Directory.GetFiles(this.Folder, this.Pattern);
                foreach (var file in existingFiles)
                {
                    await HandleFile(file, null);
                }
                this.watcher = new FileSystemWatcher(this.Folder, this.Pattern);
                this.watcher.Changed += Watcher_Changed;
                this.watcher.Created += Watcher_Changed;
                this.watcher.EnableRaisingEvents = true;
                //this.cancellationToken.Register(() =>
                //{
                //	if (this.watcher != null)
                //	{
                //		this.watcher.EnableRaisingEvents = false;
                //		this.watcher.Dispose();
                //		this.watcher = null;
                //	}
                //});
            }
            catch (Exception err)
            {
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellation)
        {
            if (this.watcher != null)
            {
                this.watcher.EnableRaisingEvents = false;
                this.watcher.Dispose();
                this.watcher = null;
            }
            await Task.FromResult(true).ConfigureAwait(false);
        }

        private async void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                var handled = await HandleFile(e.FullPath, e.ChangeType).ConfigureAwait(false);
            }
            catch (Exception err)
            {
                logger.LogError(
                    $"An error occured while trying to handle file:{e.FullPath}");
            }
        }

        public void Dispose()
        {
            if (this.watcher != null)
            {
                this.watcher.EnableRaisingEvents = false;
                this.watcher.Dispose();
                this.watcher = null;
            }
        }
    }
}
