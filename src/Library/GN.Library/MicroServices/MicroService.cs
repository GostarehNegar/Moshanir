using GN.Library.TaskScheduling;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.MicroServices
{
	public interface IMicroService : IHostedService
	{
		IMicorServiceConfiguration Config { get; }
	}
	public class MicroServiceBase : HostedService, IMicroService
	{
		public IMicorServiceConfiguration Config => throw new NotImplementedException();

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			await Task.Delay(10);
		}
	}
}
