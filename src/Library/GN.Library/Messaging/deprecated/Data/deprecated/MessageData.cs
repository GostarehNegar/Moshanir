using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Data
{
	public class MessageDataModel
	{
		public Guid Id { get; set; }
		public Guid? ReplyTo { get; set; }
		public string From { get; set; }
		public string To { get; set; }
		public long Timestamp { get; set; }
		public long? ExpiresAfter { get; set; }
		public string TypeName { get; set; }
		public string PayLoad { get; set; }
		public int? MaxDeliveries { get; set; }
		public string Topic { get; set; }
		public string Headers { get; set; }
	}
}
