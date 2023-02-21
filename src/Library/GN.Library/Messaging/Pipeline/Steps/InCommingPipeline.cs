using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Pipeline
{
	public class InCommingPipeline
	{
		private IMessageBusConfiguration config;
		public InCommingPipeline(IMessageBusConfiguration config)
		{
			this.config = config;
		}
		public Task Invoke(IMessageContext context, CancellationToken cancellationToken)
		{
			var steps = this.config.GetConfiguredSteps(Pipelines.Incomming);
			return Task.CompletedTask;
		}
	}
}
