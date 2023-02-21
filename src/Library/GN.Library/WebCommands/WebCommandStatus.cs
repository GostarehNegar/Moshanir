using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.WebCommands
{
	public enum CommandStatus
	{
		Error = -10,
		Failed = -5,
		NotStared = -1,
		InProgress = 0,
		Success = 1,
	}
}
