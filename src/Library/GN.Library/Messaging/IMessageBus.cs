using GN.Library.Shared;
using GN.Library.Messaging.Internals;
using GN.Library.Natilus.Messaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging
{
	public interface IMessageBus
	{
		IMessageContext<T> CreateMessage<T>(T message);
		ISubscriptionBuilder CreateSubscription<T>(Func<IMessageContext<T>, Task> handler = null);
		ISubscriptionBuilder CreateSubscription(Func<IMessageContext, Task> handler = null);
		IMessageBusEx Advanced();
		IProcedureCall Rpc { get; }
		//INatilusMessageContext CreateNatilusMessage(string subject, object message);
		//INatilusSubscriptionBuilder CreateNatilusSubscription(string subject);
	}
}
