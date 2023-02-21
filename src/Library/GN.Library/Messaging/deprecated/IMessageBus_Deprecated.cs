using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Deprecated
{
	public interface IMessageBus_Deprecated
	{
		Task Publish<T>(T message, CancellationToken cancellationToken = default) where T : class;
		Task<TResponse> GetResponse<TRequest, TResponse>(TRequest request, int timeout = -1, CancellationToken cancellationToken = default)
			where TRequest : class where TResponse : class;
	}
}
