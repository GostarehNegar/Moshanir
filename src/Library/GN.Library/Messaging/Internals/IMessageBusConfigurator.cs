using GN.Library.Messaging.Internals;
using GN.Library.Messaging.Pipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Internals
{
    public interface IMessageBusConfigurator
	{
		IMessageBus Bus { get; }
		IServiceCollection ServiceCollection { get; }
		IDictionary<string, object> Properties { get; }
		MessageBusOptions Options { get; }
		IConfiguration Configurations { get; }

		IMessageBusConfigurator Register(Action<ISubscriptionBuilder> configure);

		void AddPipelineStep(Func<IMessageBusConfiguration, IPipelineStep> constructor, GN.Library.Messaging.Pipeline.Pipelines pipeline, int rank = 1000);
		
	}
}
