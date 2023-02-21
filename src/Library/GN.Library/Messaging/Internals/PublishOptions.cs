using GN.Library.Messaging.Transports;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Internals
{
	public class PublishOptions
	{

		public Func<IMessageTransport, bool> TransportSelector;

		public bool TransportMatch(IMessageTransport transport)
		{
			return this.TransportSelector == null || this.TransportSelector(transport);
		}
		public bool LocalOnly { get; set; }

		public static PublishOptions GetDefault()
		{
			return new PublishOptions();
		}


	}
}
