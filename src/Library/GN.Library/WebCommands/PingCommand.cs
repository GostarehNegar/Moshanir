using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.WebCommands
{
	class PingCommand : WebCommandBase<PingCommand.RequestModel, PingCommand.ReplyModel>
	{
		public PingCommand()
		{
			this.name = "ping";
		}
		protected override CommandStatus DoHandle(RequestModel request, ReplyModel reply)
		{
			reply.Reply = AppInfo.Current.ToString();// AppHost_Deprectated.Context.Configuration.Options.AppInfo.ToString();
			return CommandStatus.Success;
		}

		public class RequestModel
		{

		}
		public class ReplyModel
		{
			public string Reply { get; set; }
			
		}
	}
}
