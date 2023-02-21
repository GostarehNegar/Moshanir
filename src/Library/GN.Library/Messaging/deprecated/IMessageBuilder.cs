using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Internals
{
	public interface IMessageBuilder
	{
		IMessageBuilder UseTopic(string topic);
		IMessageBuilder UseStream(string streamName, long version);
		IMessageContext<T> Build<T>();
	}
	class MessageBuilder : IMessageBuilder
	{
		public IMessageContext Build()
		{
			throw new NotImplementedException();
		}

		public IMessageContext<T> Build<T>()
		{
			throw new NotImplementedException();
		}

		public IMessageBuilder UseStream(string streamName, long version)
		{
			throw new NotImplementedException();
		}

		public IMessageBuilder UseTopic(string topic)
		{
			throw new NotImplementedException();
		}
	}
}
