using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Hubs
{

	[HubName("GNMessageHub")]
	public class SignalRHub : Hub, IMessageHub
	{
		class ClientInfo
		{

		}
		private static HubConnection clientConnection;



		public Task<string> Ping(BusSubscription subs)
		{
			return Task.FromResult("pong");
		}

		bool IMessageHub.IsActive => throw new NotImplementedException();

		string IMessageHub.Id => throw new NotImplementedException();

		event OnMessageReceived IMessageHub.OnMessageReceived
		{
			add
			{
				throw new NotImplementedException();
			}

			remove
			{
				throw new NotImplementedException();
			}
		}

		IMessageHub IMessageHub.Configure(bool reset, string endPointName)
		{
			throw new NotImplementedException();
		}

		Task<BusSubscription> IMessageHub.Subscribe(BusSubscription subscription)
		{
			throw new NotImplementedException();
		}

		Task<string> IMessageHub.Subscribe(string id, string topic, string selector)
		{
			throw new NotImplementedException();
		}

		string IMessageHub.Login(string userName, string password, string role)
		{
			throw new NotImplementedException();
		}

		IEnumerable<BusSubscription> IMessageHub.GetSubscriptions(Expression<Func<BusSubscription, bool>> selector)
		{
			throw new NotImplementedException();
		}

		Task IMessageHub.Publish(MessageContext message)
		{
			throw new NotImplementedException();
		}

		Task IHostedService.StartAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		Task IHostedService.StopAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
