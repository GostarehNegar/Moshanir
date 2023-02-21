using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.WebCommands
{
	public class WebCommandResponse
	{
		public CommandStatus Status { get; set; }
		public string Data { get; set; }
		public string Message { get; set; }
		public string Redirect { get; set; }
		public override string ToString()
		{
			return string.Format("Success:{0}, Message:{1}, Data:{2}",
				Status,
				Message,
				string.IsNullOrWhiteSpace(Data) ? "NULL" :
				Data.Length > 30 ? Data.Substring(0, 30) + "..." : Data
				);
		}
	}
}
