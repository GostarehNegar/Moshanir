using GN.Library.Helpers;
using GN.Library.Messaging.Data;
using GN.Library.Serialization;
using GN.Library.TaskScheduling;
//using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Hubs
{
	public interface IFileSystemHub : IMessageHub
	{

	}
	class FileSystemHub : HostedService, IFileSystemHub
	{
		protected static ILogger_Deprecated logger = typeof(FileSystemHub).GetLogger();
		private MessageIdCache messageIdCache = new MessageIdCache();

		private static int defaultTimeOut = 10 * 1000;
		private IJsonSerializer serializer;
		private CancellationToken cancellationToken;
		private FileSystemWatcher watcher;
		//private FileSystemWatcher subscriptionsWathcer;
		private ISubscriptionStore subscriptionStore;

		public string BaseFolder { get; private set; }
		public int NumberOfMessageReceived { get; private set; }
		public int NumberOfMessagesProcessed { get; private set; }
		public string FolderName { get; private set; }
		public string SubscriptionFileName { get; private set; }
		public int TimeOut { get; private set; }
		public string Endpoint { get; private set; }
		public event OnMessageReceived OnMessageReceived;
		private MessagingConfig config;
		public string Id { get; private set; }
		public bool IsActive => this.config == null ? false : this.config.UseFileSystemHub;
		public MessagingConstants Constants { get; private set; }

        public string ServerEndpoint => throw new NotImplementedException();

        public IAppUtils Utils;
		private MessagingConstants constants;
		//public List<BusSubscription> subscriptions;

		public FileSystemHub() : this(null, null) { }
		public FileSystemHub(IJsonSerializer serializer, MessagingConfig options)
		{
			init(serializer, options);
		}
		public IFileSystemHub init(IJsonSerializer serializer = null, MessagingConfig options = null, MessagingConstants constants = null)
		{
			this.serializer = serializer ?? AppHost_Deprectated.GetService<IJsonSerializer>();
            this.config = options ?? AppHost.Services.GetService<MessagingConfig>();
			this.Id = Guid.NewGuid().ToString();
			this.Constants = constants ?? MessagingConstants.Instance;
			this.Utils = AppHost.Utils;
			this.BaseFolder = this.Constants.DefaultBaseFolder;
			this.SubscriptionFileName = Path.Combine(this.Constants.DefaultBaseFolder, this.Constants.SubscriptionFileName);
			this.subscriptionStore = AppHost.Services.GetService<ISubscriptionStore>();
			this.constants = MessagingConstants.Instance;
			return this;
		}
		public FileSystemHub Configure(bool reset = true, string enpoint = null, string baseFolder = null, int? timeOut = null)
		{
			if (reset)
			{
				this.FolderName = null;
				this.Endpoint = null;
				this.BaseFolder = null;
			}
			this.BaseFolder = baseFolder ?? Constants.DefaultBaseFolder;
			this.BaseFolder = Path.GetFullPath(this.BaseFolder);
			this.Endpoint = enpoint ?? this.Endpoint;
			if (string.IsNullOrWhiteSpace(this.Endpoint))
			{
				this.Endpoint = Assembly.GetExecutingAssembly().GetName().Name;
			}
			this.Utils.ValidateEndpointName(this.Endpoint);
			this.FolderName = GetEndpointPath(this.Endpoint);
			this.TimeOut = timeOut ?? defaultTimeOut;
			this.SubscriptionFileName = Path.Combine(this.BaseFolder, this.Constants.SubscriptionFileName);
			return this;
		}

		public string GetEndpointPath(string endPointName)
		{
			return Path.Combine(this.BaseFolder, endPointName);
		}

		private Task<bool> HandleFile(string fileName)
		{
			return Task.Run<bool>(async () =>
			{
				bool result = false;
				try
				{
					var message = await UtilityHelpers.CreateTaskWithTimeOut<MessageContext>(
						work: () =>
						{
							return AppHost_Deprectated.Utils.Deserialize<MessageContext>(File.ReadAllText(fileName));
						},
						token: this.cancellationToken,
						timeOut: this.TimeOut)
						.ConfigureAwait(false);
					result = message != null;
					if (message != null && !this.cancellationToken.IsCancellationRequested && !this.messageIdCache.Contains(message.Id))
					{

						MessageReceivedEventArgs args = new MessageReceivedEventArgs(this, message, this.cancellationToken);
						/// Add this to the set of published messages
						/// so that it won't get published twice.
						///
						this.messageIdCache.Add(message.Id);

						result = this.OnMessageReceived != null
							? await this.OnMessageReceived.Invoke(args).ConfigureAwait(false)
							: false;
						result = result && await UtilityHelpers.CreateTaskWithTimeOut<bool>(
							work: () =>
							{
								File.Delete(fileName);
								return !File.Exists(fileName);
							},
							timeOut: this.TimeOut).ConfigureAwait(false);
						logger.DebugFormat(
							$"Message file successfuly processed. File:{fileName}");
					}
					else
					{
						logger.DebugFormat(
							$"FileSystemHub Message handling gracefully canceled.");
					}
				}
				catch (Exception err)
				{
					if (!(err is TimeoutException && this.cancellationToken.IsCancellationRequested))
					{
						logger.ErrorFormat(
							$"An error occured while trying to process message file. File:{fileName}, Error:{err.Message}");
					}
					else
					{

					}
				}
				return result;
			});


		}

		private async void Watcher_Created(object sender, FileSystemEventArgs e)
		{
			try
			{
				this.NumberOfMessageReceived++;
				if (!this.cancellationToken.IsCancellationRequested)
				{
					bool sucess = await HandleFile(e.FullPath).ConfigureAwait(false);
					if (sucess)
						this.NumberOfMessagesProcessed++;
				}
				else
				{

				}
			}
			catch (Exception err)
			{
				logger.ErrorFormat(
					$"An error occured while trying to process message file. File:{e.FullPath}, Error:{err.Message}");
			}

		}
		public string GetMessageFileName(MessageContext message)
		{
			return $"{message.Id}.{this.Constants.MessageFileExtension}";
		}
		public string GetMessageFileName(Guid messageId)
		{
			return $"{messageId}.{this.Constants.MessageFileExtension}";
		}
		public Task CleanUpAsync(CancellationToken cancellationToken)
		{
			return Task.Run(() =>
			{
				foreach (var file in Directory
						.GetFiles(this.BaseFolder, "*." + MessagingConstants.Instance.MessageFileExtension, SearchOption.AllDirectories))
				{
					try
					{
						if (cancellationToken.IsCancellationRequested)
							break;
						this.constants = this.constants ?? MessagingConstants.Instance;
						var max = this.constants.MaxAgeOfMessageInSeconds;

						if ((DateTime.UtcNow - File.GetCreationTimeUtc(file)).TotalSeconds > max)
						{
							var message = this.serializer.Deserialize<MessageContext>(File.ReadAllText(file));
							if (DateTime.UtcNow > message.GetExpirationDate(true))
							{
								File.Delete(file);
							}
						}
					}
					catch (Exception err)
					{
						logger.ErrorFormat(
							"An error occured in FileSystem cleanup. Err: {0}", err.Message);

					}
				}
			});


		}

		public Task Publish(MessageContext message, BusSubscription subscription,bool toServer = true, bool toCleints = true)
		{
			bool dontPublishToSelf = true;
			return Task.Run(async () =>
			{
				if (message == null)
					return;
				if (message.IsReply() && message.ReplyTo.HasValue && AppHost_Deprectated.Utils.ValidateEndpointName(message.From))
				{
					try
					{
						var folder = GetEndpointPath(message.From);
						if (!Directory.Exists(folder))
							Directory.CreateDirectory(folder);
						var fileName = Path.Combine(folder, GetMessageFileName(message.ReplyTo.Value));
						if (File.Exists(fileName))
							File.Delete(fileName);

					}
					catch (Exception err)
					{
						logger.ErrorFormat(
							"An error occured while trying to process messge acknowledement. Err: {0}", err.Message);
					}


				}
				var sub = subscription;
				var toendpoint = string.IsNullOrWhiteSpace(message.To)
					? subscription?.Endpoint
					: message.To;
				if (AppHost.Utils.ValidateEndpointName(toendpoint) && !message.IsReply())
				{
					try
					{
						var ret = await UtilityHelpers.CreateTaskWithTimeOut<bool>(() =>
						{
							var folder = GetEndpointPath(toendpoint);
							if (!Directory.Exists(folder))
								Directory.CreateDirectory(folder);
							var fileName = Path.Combine(folder, GetMessageFileName(message));
							var text = message.Serialize();
							System.IO.File.WriteAllText(fileName, text);
							this.messageIdCache.Add(message.Id);
							return true;
						}).ConfigureAwait(false);
					}
					catch (Exception err)
					{
						logger.ErrorFormat(
							"An error occured while trying to publish message. Err: {0}", err.Message);
					}
				}
			}, this.cancellationToken);
		}


		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			this.cancellationToken = cancellationToken;
			try
			{
				/// Start Up
				/// 
				this.Utils.ValidateEndpointName(this.Endpoint);
				if (!Directory.Exists(this.FolderName))
					Directory.CreateDirectory(this.FolderName);

				/// Handle existing files
				/// 
				foreach (var file in Directory.GetFiles(this.FolderName,
					$"*.{this.Constants.MessageFileExtension}",
					SearchOption.TopDirectoryOnly))
				{
					if (this.cancellationToken.IsCancellationRequested)
						break;
					await this.HandleFile(file).ConfigureAwait(false);
					//this.HandleFile(file);
				}
				if (!this.cancellationToken.IsCancellationRequested)
				{
					this.watcher = new FileSystemWatcher(this.FolderName, $"*.{this.Constants.MessageFileExtension}")
					{
						EnableRaisingEvents = true

					};
					this.watcher.Created += Watcher_Created;
					this.subscriptionStore.StartAsync(cancellationToken)
						.ConfigureAwait(false).GetAwaiter().GetResult();
					logger.InfoFormat(
						$"FileSystemHub is started. Folder:{this.FolderName}");
				}
				while (!this.cancellationToken.IsCancellationRequested)
				{
					await this.CleanUpAsync(this.cancellationToken).ConfigureAwait(false);
					try
					{
						await Task.Delay(2 * 60 * 1000, this.cancellationToken).ConfigureAwait(false);
					}
					catch { }
				}

				//this.cancellationToken.Register(() =>
				//{
				if (this.watcher != null)
				{
					this.watcher.EnableRaisingEvents = false;
					this.watcher.Dispose();
					this.watcher = null;
				}
				this.subscriptionStore.StopAsync(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult();
				//});
				//await Task.Delay(-1, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception err)
			{
				logger.ErrorFormat(
					$"An error occured in FileSystemHub Start. Error:{err.Message}");
				if (this.watcher != null)
				{
					this.watcher.EnableRaisingEvents = false;
					this.watcher.Dispose();
					this.watcher = null;
				}
				throw;
			}

			//return Task.Delay(-1, cancellationToken);
		}

		IMessageHub IMessageHub.Configure(bool reset, string endPointName)
		{
			return this.Configure(reset: reset, enpoint: endPointName);
		}

		public async Task<List<BusSubscription>> GetSubscriptions(bool refersh = false)
		{
			var items = await this.subscriptionStore.GetItemsAsync(refersh).ConfigureAwait(false);
			return items.ToList();
		}

		public Task<BusSubscription> Subscribe(Action<BusSubscription> configurer)
		{
			var subscription = new BusSubscription();
			subscription.Endpoint = this.Endpoint;
			configurer?.Invoke(subscription);
			return this.Subscribe_Deprecated(subscription);
		}

		public async Task<BusSubscription> Subscribe_Deprecated(BusSubscription subscription)
		{
			return await this.subscriptionStore.UpsertAsync(subscription).ConfigureAwait(false);
		}

		public override string ToString()
		{
			return $"FileSystemHub. Endpoint:{this.Endpoint}";
		}

		string IMessageHub.Login(string userName, string password, string role)
		{
			throw new NotImplementedException();
		}


		public Task Subscribe(BusSubscription subscription)
		{
			return Task.FromResult(true);
		}

        public Task<string> GetServerEndpoint()
        {
            throw new NotImplementedException();
        }

        public Task Publish(MessageContext message, string endpoint, bool publishToServer)
        {
            throw new NotImplementedException();
        }

        Task<bool> IMessageHub.Publish(MessageContext message, string endpoint, bool publishToServer)
        {
            throw new NotImplementedException();
        }
    }
}
