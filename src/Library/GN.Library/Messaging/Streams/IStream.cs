using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Streams
{
    public class ReplayContext
	{
		public bool Stop { get; set; }
		public MessagePack[] Events {get;set;}
		public long Remaining { get; set; }
		public long Position { get; set; }
		public long TotalCount { get; set; }
	}
	public interface IStream : IDisposable
	{
		Task<IStream> OpenAsync();
		Task<IEnumerable<MessagePack>> SaveAsync(IEnumerable<MessagePack> events, long? expectedVersion = null, CancellationToken cancellationToken = default);
		Task Replay(Func<MessagePack, Task<bool>> callBack, long? pos = null, CancellationToken cancellationToken = default);
		Task ReplayEx(Func<ReplayContext,Task> callBack, long? pos = null, int? chunkSize=1000, CancellationToken cancellationToken = default);
	}
}
