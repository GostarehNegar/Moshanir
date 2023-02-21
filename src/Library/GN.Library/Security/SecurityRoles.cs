using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Security
{
	[Flags]
	public enum SecurityRoles
	{
		None = 0,
		AdimnService = 1,
		Service = 2,
		User = 4,
		AnotherService = 8
	}
}
