using GN.Library.CommandLines_deprecated;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.WebCommands
{
	class CommandLineWebCommand : WebCommandBase<CommandLineWebCommand.RequestModel, CommandLineWebCommand.ReplyModel>
	{
		public CommandLineWebCommand()
		{
			this.name = "commandline";
		}
		//private async Task<CommandStatus> _Handle(RequestModel request, ReplyModel reply)
		//{
		//	var result = CommandStatus.Success;
			
		//	using (var scope= AppHost.Context.Push())
		//	{
		//		var message = AppHost.Bus.CreateMessage(new CommandLines.CommandLineMessage { CommandLine = request.Args });
		//		var awaiter = AppHost.Bus.CreateWaiter<CommandLines.CommandLineMessageReply>(async ctx => {
		//			return await Task.FromResult(ctx.ReplyTo == message.Id);
		//		});
		//		try
		//		{
		//			await message.Publish();
		//			var rep = (await awaiter.GetTask()).GetMessage();
		//			reply.Log = rep.Log;
		//			reply.Context = rep.Context;
		//			if (!string.IsNullOrEmpty(rep.Error))
		//			{
		//				result = CommandStatus.Error;
		//			}
		//		}
		//		catch (Exception err)
		//		{
		//			reply.Log = $"Error :{err.GetBaseException().Message}";
		//			result = CommandStatus.Error;
		//		}
		//	}


		//	return result;
		//}
		protected override CommandStatus DoHandle(RequestModel request, ReplyModel reply)
		{
			//return _Handle(request, reply).ConfigureAwait(false).GetAwaiter().GetResult();
			using (var scope = AppHost.Context.Push())
			{
				var commandLine = new CommandLineApplicationEx();
				try
				{
					commandLine.WriteLine("");
					commandLine.WriteLine("");
					commandLine.Execute(request.Args, request.Context)
						.TimeOutAfter(30 * 1000)
						.ConfigureAwait(false).GetAwaiter().GetResult();
					commandLine.Write("\r\n======================\r\nFinished Executing: '{0}'\r\n", request.Args);
				}
				catch (Exception err)
				{
					commandLine.WriteLine("$$$ Error: {0}\r\n {1} \r\n", err.Message,
						err.InnerException?.Message);
				}
				reply.Log = commandLine.OutString;
				reply.Context = commandLine.Context;
			}
			return CommandStatus.Success;
		}

		public class RequestModel
		{
			public string Args { get; set; }
			public CommandLineContext Context { get; set; }
		}
		public class ReplyModel
		{
			public string Log { get; set; }
			public CommandLineContext Context { get; set; }

		}
	}
}
