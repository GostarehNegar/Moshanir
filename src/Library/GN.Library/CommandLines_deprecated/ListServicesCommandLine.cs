using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace GN.Library.CommandLines_deprecated
{
	//class ListServicesCommandLine : CommandLine
	//{
	//	public override void DoConfigure(CommandLineApplicationEx app)
	//	{

	//		this.Command = app.Command("Services", cfg =>
	//		{
	//			cfg.Description = "List the set of services currently running on this server.";
	//			cfg.Argument("name", "name of service");
	//		});
	//	}

	//	public override Task<int> DoExecute(CommandLineApplicationEx app)
	//	{
	//		var i = 1;
	//		app.WriteLine("List registered services:");
	//		AppHost.GetServices<IHostedService>().ToList().ForEach(srv =>
	//		{
	//			app.WriteLine($"{i}:\t {srv.GetType().Name}");
	//			i++;
	//		});
	//		i = 0;
	//		app.WriteLine("List of ScheduledTasks:");
	//		AppHost.GetServices<TaskScheduling.IScheduledTask>().ToList().ForEach(srv =>
	//		{
	//			app.WriteLine($"{i}:\t {srv.GetType().Name}");
	//		});


	//		return Task.FromResult(0);
	//		//throw new NotImplementedException();
	//	}
	//}
}
