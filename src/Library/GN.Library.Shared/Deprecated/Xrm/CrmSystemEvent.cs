using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Contracts_Deprecated
{
	public class XrmSystemMessageModel
	{
		public Guid? SystemEventId { get; set; }
		public string Topic { get; set; }
		public string Mode { get; set; }

		public string MessageType { get; set; }
		public string Message { get; set; }

		public string Str1 { get; set; }
		public string Str2 { get; set; }
		public string StrResult { get; set; }
		public string Response { get; set; }

		public string Url1 { get; set; }
		public string Url2 { get; set; }

		public int? Int1 { get; set; }
		public int? Int2 { get; set; }
		public int? IntResult { get; set; }

		public DateTime? DateTime1 { get; set; }
		public DateTime? DateTime2 { get; set; }
		public DateTime? DateTimeResult { get; set; }

		public DateTime? Date1 { get; set; }
		public DateTime? Date2 { get; set; }
		public DateTime? DateResult { get; set; }


		public double? Float1 { get; set; }
		public double? Float2 { get; set; }
		public double? FloatResult { get; set; }

		public decimal? Decimal1 { get; set; }
		public decimal? Decimal2 { get; set; }
		public decimal? DecimalResult { get; set; }

		public string Error { get; set; }
		public string Log { get; set; }

		public bool IsReady { get; set; }
		public bool Failed { get; set; }
		public bool Completed { get; set; }

		public bool IsCommand()
		{
			return !string.IsNullOrWhiteSpace(this.Mode) && this.Mode.ToLowerInvariant().Contains("command");
		}
		public bool IsEvent()
		{
			return !IsCommand();
		}

		public XrmSystemMessageReplyModel CreateReply()
		{
			return new XrmSystemMessageReplyModel
			{
				Request = this,
				Topic = this.Topic,
				Int1 = this.Int1,
				Int2 = this.Int2,
				Str1 = this.Str1,
				Str2 = this.Str2,
				Float1 = this.Float1,
				Float2 = this.Float2,
				Decimal1 = this.Decimal1,
				Decimal2 = this.Decimal2,
				Date1 = this.Date1,
				Date2 = this.Date2,
				Message = this.Message,
				MessageType = this.MessageType,
				Url1 = this.Url1,
				Url2 = this.Url2,
			};
		}


	}
	public class XrmSystemMessageReplyModel : XrmSystemMessageModel
	{
		public XrmSystemMessageModel Request { get; set; }

		public Guid? GetRequestId()
		{
			return this.Request?.SystemEventId;
		}
	}
}
