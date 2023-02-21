using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Contracts_Deprecated.Gateway
{
	public class AddMicroServiceRequest
	{
		public string Urls { get; set; }
	}
	public class AddMicroServiceResult
	{
		public string Id { get; set; }

	}
}
