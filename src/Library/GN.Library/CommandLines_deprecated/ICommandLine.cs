using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace GN.Library.CommandLines_deprecated
{
	public interface ICommandLine
	{
		void Configure(CommandLineApplicationEx parent);
		Task<int> Execute(CommandLineApplicationEx command);
	}
	public abstract class CommandLine : ICommandLine
	{
		public CommandLineApplicationEx Command;
		protected abstract string Name { get; }
		public void Configure(CommandLineApplicationEx parent)
		{
			var command = new CommandLineApplicationEx
			{
				Name = Name,
				Parent = parent
			};
			this.DoConfigure(command);
			if (command.Name != null)
			{
				command.Extend();
				parent.AddCommand(command);
				this.Command = command;
				command.OnExecute(() =>
				{
					return this.Execute(command);
				});
			}
		}
		public abstract void DoConfigure(CommandLineApplicationEx command);
		public Task<int> Execute(CommandLineApplicationEx command)
		{
			return DoExecute(command);
		}
		public abstract Task<int> DoExecute(CommandLineApplicationEx command);
	}
	class TestCommand : CommandLine
	{
		protected override string Name => "test";
		public CommandOption greeting;
		public override void DoConfigure(CommandLineApplicationEx command)
		{
			command.Name = "test";
			this.greeting = command.Option("--jjj", "", CommandOptionType.NoValue);
			command.Command("hey", cfg => { });
		}

		public override Task<int> DoExecute(CommandLineApplicationEx ctx)
		{
			return Task.FromResult(0);
		}
	}
}
