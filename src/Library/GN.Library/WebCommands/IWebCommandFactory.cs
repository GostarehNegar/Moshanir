using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.WebCommands
{
	public interface IWebCommandFactory
	{
		IWebCommand Create(string name);
	}

	class WebCommandFactory : IWebCommandFactory
	{
		public IWebCommand Create(string name)
		{
			var commands = AppHost.GetServices<IWebCommand>();
			return commands.FirstOrDefault(x => string.Compare(name, x.Name, true) == 0);
		}
	}
}
