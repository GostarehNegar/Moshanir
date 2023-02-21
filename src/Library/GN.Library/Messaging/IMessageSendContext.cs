using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging
{
	public interface IMessageSendContext
	{
		MessageContext Message { get; }
	}
	public class MessageSendContext : IMessageSendContext
	{
		public MessageContext Message { get; private set; }

		public MessageSendContext(MessageContext context, Task sendTask)
		{

		}
	}

}
