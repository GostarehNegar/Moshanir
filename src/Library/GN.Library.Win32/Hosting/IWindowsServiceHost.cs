using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Win32.Hosting
{
	public interface IWindowsServiceHost : IHost
	{
		IWebHost GetHost();
		void Run();
	}
}
