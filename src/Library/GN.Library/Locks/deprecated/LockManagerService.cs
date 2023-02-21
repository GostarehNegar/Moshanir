using GN.Library.Contracts;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Locks
{
	class LockManagerService : IHostedService
	{
		private bool? remote;
		//private ConcurrentDictionary

		public async Task<BusLock> TryLock(string key)
		{
			BusLock result = null;
			if (!this.remote.HasValue || remote.Value)
			{
				try
				{
					var res = await AppHost.Bus.GetResponse<LockRecordCommand, LockRecordCommandResult>(
						new LockRecordCommand
						{
							Key = key
						});
					if (res != null)
					{
						remote = true;
						if (res.Key == key)
						{
							result = new BusLock
							{
								Key = res.Key
								
							};
						}
					}
				}
				catch (Exception)
				{
					if (!remote.HasValue)
						remote = false;
				}
			}
			if (remote.HasValue && !remote.Value && result == null)
			{
				/// There is no remote service. 
			}
			
			

			return result;
		}
		public async Task Test()
		{
			await AppHost.Bus.GetResponse<LockRecordCommand, LockRecordCommandResult>(new LockRecordCommand { });
		}
		public Task StartAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
