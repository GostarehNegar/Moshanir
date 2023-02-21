using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace GN.Library.CommandLines_deprecated
{
	public class CommandLineContext
	{
		public Dictionary<string, object> Headers { get; set; }
		public CommandLineContext()
		{
			this.Headers = new Dictionary<string, object>();
		}

	}
	public class CommandLineApplicationEx : CommandLineApplication
	{
		public static CommandLineApplicationEx Instance = new CommandLineApplicationEx();

		private StringWriter writer = null;
		public string OutString { get { return Writer?.ToString(); } }
		public CommandLine Current { get; set; }
		private CommandLineContext context;

		public CommandLineContext Context
		{
			get
			{
				if (this.context == null)
				{
					this.context = (this.Parent as CommandLineApplicationEx)?.Context;
				}
				if (this.context == null)
				{
					this.context = new CommandLineContext();
				}
				return this.context;
			}
		}

		public StringWriter Writer
		{
			get
			{
				if (this.writer == null)
				{
					if (this.Parent as CommandLineApplicationEx != null)
						this.writer = (this.Parent as CommandLineApplicationEx).Writer;
				}
				if (this.writer == null)
					this.writer = new StringWriter();
				return this.writer;
			}
		}
		public CommandLineApplicationEx()
		{

		}
		public CommandLineApplicationEx(CommandLineApplication a)
		{
			this.Name = a.Name;
			this.Description = a.Description;
			this.AllowArgumentSeparator = a.AllowArgumentSeparator;
			this.ExtendedHelpText = a.ExtendedHelpText;
			this.FullName = a.FullName;
			this.Out = a.Out;
			this.Invoke = a.Invoke;
			this.Options.AddRange(a.Options);
			this.Arguments.AddRange(a.Arguments);
			this.RemainingArguments.AddRange(a.RemainingArguments);
			this.Parent = a.Parent;
			this.Commands.AddRange(a.Commands.Select(x => new CommandLineApplicationEx(x)).ToArray());
		}
		public void Initialize(bool reset = false)
		{

			if (this.Commands.Count == 0 || reset)
			{
				var commands = AppHost.GetServices<CommandLine>();
				commands.ToList().ForEach(x => x.Configure(this));
				this.Name = "$";
				this.HelpOption("-?|-h|--help");
			}
		}
		public Task<int> Execute(string args, CommandLineContext context)
		{
			this.Out = this.Writer;
			this.context = context ?? new CommandLineContext();
			Initialize();
			this.Execute(args.SplitCommandLine().ToArray());
			
			this.Out.Flush();
			return Task.FromResult(0);
		}
		public void WriteLine(string fmt, params object[] args)
		{
			this.Writer?.WriteLine(fmt, args);
		}
		public void Write(string fmt, params object[] args)
		{
			this.Writer?.Write(fmt, args);
		}

		public CommandArgument GetArgument(string name)
		{
			return this.Arguments.Where(x => x.Name == name).FirstOrDefault();
		}
		public void Extend()
		{
			this.Out = this.Parent?.Out;
			if (this.OptionHelp == null)
				this.HelpOption("-?|-h|--help");
			for (int i = 0; i < this.Commands.Count; i++)
			{
				var command = this.Commands[i];
				if (command.GetType() == typeof(CommandLineApplication))
				{
					this.Commands[i] = new CommandLineApplicationEx(command);
				}
				(this.Commands[i] as CommandLineApplicationEx)?.Extend();
			}
		}
		public void AddCommand(CommandLineApplicationEx command)
		{
			command.Parent = this;
			this.Commands.Add(command);
		}
		public CommandLineApplicationEx Command(string name, Action<CommandLineApplicationEx> cfg, bool trueOnUnexpectedArgs = true)
		{

			var command = new CommandLineApplicationEx();
			command.Parent = this;
			command.Name = name;
			cfg(command);
			this.Commands.Add(command);
			//var help = this.GetHelpText();
			return command;
		}



	}
}
