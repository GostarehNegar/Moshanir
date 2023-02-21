using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Contracts_Deprecated.Gateway
{
	public class MicroServiceModel
	{
		public string Name { get; set; }
		public string Version { get; set; }
		public string PathPatterns { get; set; }
		public string RemoveFormPath { get; set; }
	}
	public class ApiPingResultModel
	{
		//public string Name { get; set; }
		//public string Version { get; set; }
		//public string PathPatterns { get; set; }
		//public string RemoveFormPath { get; set; }
		public MicroServiceModel[] MicroServices { get; set; }
	}
}
