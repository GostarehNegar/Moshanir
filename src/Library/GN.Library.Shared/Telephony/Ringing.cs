using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Contracts_Deprecated.Telephony
{

	public class CallStatusChange
	{
		public string Caller { get; set; }
		public string Called { get; set; }
		public string CallId { get; set; }
		public DateTime Time { get; set; }

		public string Status { get; set; }
		public int Duration { get; set; }
		public override string ToString()
		{
			return $"Riniging. Caller:{Caller}, Called:{Called}";
		}

	}
	public class Ringing
	{
		public string Caller { get; set; }
		public string Called { get; set; }
		public string CallId { get; set; }
		public string ChannelName { get; set; }

		public string Direction { get; set; }

		public bool IsVirtual { get; set; }
		public bool IsFromInternal { get; set; }
		public bool IsLocal { get; set; }


		public DateTime Time { get; set; }

		public string Status { get; set; }

		public int Duration { get; set; }

		public DateTime? RingingTime { get; set; }
		public DateTime? RingTime { get; set; }
		public DateTime? UpTime { get; set; }
		public DateTime? DownTime { get; set; }

		public DateTime? HangupTime { get; set; }
		public override string ToString()
		{
			return $"Riniging. Caller:{Caller}, Called:{Called}";
		}
	}
	public class Ring
	{
		public string Caller { get; set; }
		public string Called { get; set; }
		public string CallId { get; set; }
		public DateTime Time { get; set; }
		public int Duration { get; set; }
		public override string ToString()
		{
			return $"Ring. Caller:{Caller}, Called:{Called}";
		}
	}
	public class Up
	{
		public string Caller { get; set; }
		public string Called { get; set; }
		public string CallId { get; set; }
		public int Duration { get; set; }
		public DateTime Time { get; set; }
		public override string ToString()
		{
			return $"Up. Caller:{Caller}, Called:{Called}";
		}
	}

	public class Down
	{
		public string Caller { get; set; }
		public string Called { get; set; }
		public string CallId { get; set; }
		public int Duration { get; set; }
		public DateTime Time { get; set; }
		public override string ToString()
		{
			return $"Down. Caller:{Caller}, Called:{Called}";
		}


	}
	public class HanUp
	{
		public string Caller { get; set; }
		public string Called { get; set; }
		public string CallId { get; set; }
		public int Duration { get; set; }
		public DateTime Time { get; set; }
		public override string ToString()
		{
			return $"Down. Caller:{Caller}, Called:{Called}";
		}


	}

}
