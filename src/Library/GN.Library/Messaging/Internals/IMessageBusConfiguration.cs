using GN.Library.Messaging.Pipeline;
using System.Collections.Generic;

namespace GN.Library.Messaging.Internals
{
    public interface IMessageBusConfiguration
	{
		IMessagingServices ServiceProvider { get; }
		IEnumerable<IPipelineStep> GetConfiguredSteps(GN.Library.Messaging.Pipeline.Pipelines pipeline);
		//MessageBusOptions Options { get; }
		
	}
}
