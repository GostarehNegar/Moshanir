using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace GN.Library.Messaging.Streams
{
	public interface IProjection
	{
		Task<LogicalMessage[]> Project(LogicalMessage[] source, CancellationToken cancellationToken = default);
	}

	class Projector : IProjection
	{
		private Func<LogicalMessage, Task<LogicalMessage>> projector;
		public Projector(Func<LogicalMessage, Task<LogicalMessage>> projector)
		{
			this.projector = projector;
		}
		public async Task<LogicalMessage[]> Project(LogicalMessage[] source, CancellationToken cancellationToken = default)
		{
			await Task.CompletedTask;
			if (this.projector == null)
				this.projector = s => Task.FromResult<LogicalMessage>(null);
			var result = new List<object>();
			source.ToList()
				.ForEach(async x =>
				{
					result.Add(await this.projector(x));
				});
			return source.Where(x => x != null)
				.ToArray();
		}
	}

}
