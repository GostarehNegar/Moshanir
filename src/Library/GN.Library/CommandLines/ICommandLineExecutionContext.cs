using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.CommandLines
{
	public interface ICommandLineExecutionContext
	{
		IDictionary<string,object> Properties { get; }
	}
}
