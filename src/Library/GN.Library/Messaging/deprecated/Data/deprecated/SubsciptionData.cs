using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Data
{
	public class SubsciptionDataModel
	{
		public string Id { get; set; }
		public string Topic { get; set; }
		public string Selector { get; set; }
	}
}
