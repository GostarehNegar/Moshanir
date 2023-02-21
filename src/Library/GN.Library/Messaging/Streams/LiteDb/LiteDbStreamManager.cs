using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Streams.LiteDb
{
	public class LiteDbStreamManager : IStreamManager
	{
		public string GetStreamsFolder()
		{
			var folder = Path.GetFullPath(
				Path.Combine(
					Path.GetDirectoryName(this.GetType().Assembly.Location), "streams"));
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
			return folder;
		}
		public string GetFileName(string streamName)
		{

			return Path.Combine(GetStreamsFolder(), $"{streamName}") + ".strm";


		}

		public string GetConnectionString(string streamName)
		{
			return $"Filename ={GetFileName(streamName)}";

		}
		public async Task<IStream> GetStream(string streamName,  bool autoCreate = false)
		{
			await Task.CompletedTask;

			if (File.Exists(GetFileName(streamName)) || autoCreate)
			{
				return new LiteDbStream(GetConnectionString(streamName));
			}
			return null;
		}
	}
}
