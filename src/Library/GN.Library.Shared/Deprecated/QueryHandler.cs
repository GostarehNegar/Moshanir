using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Contracts_Deprecated
{
	public class QueryHandler
	{
		public string TopicName { get; set; }
		public string Stream { get; set; }
		//public string StreamId { get; set; }
	}
	public class QueryHandlerReply
	{
		public string EndpointName { get; set; }
		public bool IsReady { get; set; }
	}

}
