using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.WebCommands
{
	public interface IWebCommand
	{
		WebCommandResponse Handle(WebCommandRequest request);
		
		string Name { get; }
	}
}
