using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.CommandLines
{
	/// <summary>
	/// Factory that creates the execution method for a command line.
	/// This is specially used when commands are created based on contracts
	/// and should be executed on a remote service.
	///
	/// </summary>
	public interface ICommandOnExecuteFactory
	{
		Func<ICommandLineExecutionContext, CancellationToken, Task<int>> Create<T>() where T : class;
	}
}
