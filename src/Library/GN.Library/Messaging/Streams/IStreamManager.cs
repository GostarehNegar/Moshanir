using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Streams
{
	public interface IStreamManager
	{
		Task<IStream> GetStream(string streamName, bool autoCreate = false);
		//Task<bool> DeleteStream(string streamName, string streamId);
	}
}
