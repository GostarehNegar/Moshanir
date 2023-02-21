using GN.Library.Messaging;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.CommandLines
{
	public class CommandLineMessage
	{
		public string CommandLine { get; set; }
	}
	public class CommandLineMessageReply
	{
		public string Log { get; set; }
		public CommandLineContext Context { get; set; }
		public string Error { get; set; }
	}
	
	public class CommandLineHandlerService : IHostedService
	{
		private IMessageBus bus;
		public CommandLineHandlerService()
		{

		}
		private async Task HandleCommand(MessageContext<CommandLineMessage> ctx)
		{
			using (var scope = AppHost.Context.Push())
			{
				var reply = new CommandLineMessageReply();
				var commandLine = new CommandLineApplicationEx();
				try
				{
					var command = ctx.GetMessage().CommandLine;
					var context = new CommandLineContext();
					commandLine.WriteLine("");
					commandLine.WriteLine("");
					await commandLine.Execute(ctx.GetMessage().CommandLine, context)
						.TimeOutAfter(30 * 1000)
						.ConfigureAwait(false);//.GetAwaiter().GetResult();
					//commandLine.Write("\r\n======================\r\nFinished Executing: '{0}'\r\n", request.Args);
				}
				catch (Exception err)
				{
					commandLine.WriteLine("$$$ Error: {0}\r\n {1} \r\n", err.Message,
						err.InnerException?.Message);
					reply.Error = err.GetBaseException().Message;
				}
				reply.Log = commandLine.OutString;
				reply.Context = commandLine.Context;
				ctx.CreateResponse<CommandLineMessageReply>(reply);
				await ctx.Reply(reply);
			}
			
		}
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await Task.Delay(100);
			
			await AppHost.Bus.SubscribeAsync<CommandLineMessage>(cfg =>
			{
				cfg.Handler = async ctx => {
					if (ctx.GetMessageEx<CommandLineMessage>() != null)
					{
						await this.HandleCommand(ctx.Cast<CommandLineMessage>() as MessageContext<CommandLineMessage>);
					}
				};

			});
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
