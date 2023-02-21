using GN.Library.Helpers;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Data
{
    public interface ISubscriptionStore : IHostedService
    {
        Task<BusSubscription> UpsertAsync(BusSubscription subscription, bool writeFile = true);
        void Delete(BusSubscription subscription);
        Task Clear();
        Task<IEnumerable<BusSubscription>> GetItemsAsync(bool refresh = false);
        IEnumerable<BusSubscription> GetItems(bool refresh = false);
        Task Save();
        
    }

    public class FileSystemSubscriptionStore : ISubscriptionStore
    {
        private int DefaultTimeOut = 20 * 1000;
        private Encoding encoding;
        protected static ILogger_Deprecated logger = typeof(FileSystemSubscriptionStore).GetLogger();
        private string FileName;
        private List<BusSubscription> items;
        private JsonFolderWatcher<List<BusSubscription>> watcher;
        public Guid Id = Guid.NewGuid();
        public FileSystemSubscriptionStore()
        {
            this.FileName = Path.Combine(MessagingConstants.Instance.DefaultBaseFolder, "subscriptions.dat");
            if (!Directory.Exists(Path.GetDirectoryName(this.FileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(this.FileName));
        }
        private Task Watcher_OnChange(IJsonFolderWatcher<List<BusSubscription>> sender,
            string fileName, WatcherChangeTypes? changeType,
            List<BusSubscription> value)
        {
            if (AppDomain.CurrentDomain.FriendlyName == "client2")
            {
                var gggg = 1;
            }
            this.items = value.ToList();
            return Task.FromResult(true);
        }

        public FileSystemSubscriptionStore Configure(string path)
        {
            this.FileName = path;
            return this;
        }

        private async Task<bool> DoWrite()
        {
            return await UtilityHelpers.CreateTaskWithTimeOut<bool>(() =>
            {
                var text = AppHost_Deprectated.Utils.Serialize(this.items);
                if (this.encoding == null)
                    File.WriteAllText(this.FileName, text);
                else
                    File.WriteAllText(this.FileName, text, this.encoding);
                return true;
            },
            timeOut: this.DefaultTimeOut).ConfigureAwait(false);
        }
        private async Task<bool> DoRead()
        {
            if (!File.Exists(this.FileName))
            {
                this.items = new List<BusSubscription>();
                return true;
            }
            else
            {
                return await UtilityHelpers.CreateTaskWithTimeOut<bool>(() =>
                {
                    var text = "";
                    if (this.encoding == null)
                        text = File.ReadAllText(this.FileName);
                    else
                        text = File.ReadAllText(this.FileName, this.encoding);
                    try
                    {
                        this.items = AppHost_Deprectated.Utils.Deserialize<List<BusSubscription>>(text);
                    }
                    catch { }
                    this.items = this.items ?? new List<BusSubscription>();
                    return true;
                },
                timeOut: this.DefaultTimeOut).ConfigureAwait(false);
            }

        }

        private async Task<List<BusSubscription>> DoGetItemsAsync(bool refresh = false)
        {
            if (this.items == null || refresh)
            {
                await DoRead().ConfigureAwait(false);
                this.items = this.items ?? new List<BusSubscription>();
            }
            return this.items;

        }

        public async Task<BusSubscription> UpsertAsync(BusSubscription subscription, bool writeFile = true)
        {
            await DoGetItemsAsync();
            if (this.items == null)
            {
                throw new Exception("Unexpected Items is NULL");
            }
            if (subscription == null)
            {
                throw new Exception($"Invalid Endpoint :{subscription?.Endpoint}");
            }
            if (!AppHost_Deprectated.Utils.ValidateEndpointName(subscription.Endpoint))
            {
                throw new Exception($"Invalid Endpoint :{subscription.Endpoint}");
            }
            var result = this.items.FirstOrDefault(x => x.Equals(subscription));
            if (result == null)
            {
                lock (this.items)
                {
                    items.Add(subscription);
                }
                result = subscription;
            }
            else
            {
                result.AddHeaders(subscription.Headers);
            }
            if (writeFile)
                await DoWrite().ConfigureAwait(false);
            return result;
        }
        public Task Clear()
        {
            this.items.Clear();
            return DoWrite();
        }
        public void Delete(BusSubscription subscription)
        {
            throw new NotImplementedException();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var folder = Path.GetDirectoryName(this.FileName);
            var name = Path.GetFileName(this.FileName);

            if (AppDomain.CurrentDomain.FriendlyName == "client2")
            {
                var ffff = 1;
            }
            if (this.watcher == null)
            {
                await this.DoRead().ConfigureAwait(false);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                this.watcher = new JsonFolderWatcher<List<BusSubscription>>(folder, name, this.encoding);
                this.watcher.OnChange += Watcher_OnChange;
                await this.watcher.StartAsync(cancellationToken).ConfigureAwait(false);
            }


        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (this.watcher != null)
            {
                await this.watcher.StopAsync(cancellationToken);
                this.watcher = null;
            }

        }
        public async Task<IEnumerable<BusSubscription>> GetItemsAsync(bool refresh = false)
        {
            var result = await DoGetItemsAsync(refresh).ConfigureAwait(false);
            return result.AsEnumerable();
        }

        public IEnumerable<BusSubscription> GetItems(bool refresh = false)
        {
            return this.DoGetItemsAsync().ConfigureAwait(false).GetAwaiter().GetResult().AsEnumerable();
        }

        public Task AddRange(IEnumerable<BusSubscription> subscriptions)
        {
            throw new NotImplementedException();
        }

        public Task Save()
        {
            return DoWrite();
        }
    }
}
