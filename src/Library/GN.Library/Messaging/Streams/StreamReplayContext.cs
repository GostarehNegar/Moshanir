using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Streams
{
    public class StreamReplayContext
	{
		public MessagePack[] Events { get; set; }
		public long RemaininCount { get; set; }
		public long Position { get; set; }
		public bool Stop { get; set; }
	}
}
