using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Transports
{

	
	public interface IMessageTransport
	{
		string Name { get; }
		Task<bool> Subscribe(IMessageBusSubscription topic);
		Task Publish(IMessageContext message);
		//void SetOnReceive(Func<IMessageTransport,object,IDictionary<string,object>,Task> handler);
		void Init(IMessageBus bus, Func<IMessageTransport, object, IDictionary<string, object>, Task> handler);
		bool Matches(IMessageContext context);
		bool IsConnected(int timeOut);
		Task StopAsync(CancellationToken token);

		
	}
}
