using System;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Pipeline
{
	public interface IMessageBusPipeline
	{
		Task Invoke(IPipelineContext context);
	}
	class MessageBusPipeline : IMessageBusPipeline
	{
		public Task Invoke(IPipelineContext context)
		{
			Task do_invoke(int idx)
			{
				if (idx == context.Steps.Length)
					return Task.CompletedTask;
				return context.Steps[idx].Handle(context, ctx =>
				{
					return do_invoke(idx + 1);
				});
			}
			return do_invoke(0);
		}
	}
}
