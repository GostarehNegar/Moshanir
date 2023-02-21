using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Pipeline.Steps
{
    class VersionableEventStep: IPipelineStep
    {
        private readonly MessageBus bus;

        public VersionableEventStep(MessageBus bus)
        {
            this.bus = bus;
        }

        public Task Handle(IPipelineContext ctx, Func<IPipelineContext, Task> next)
        {
            return Task.CompletedTask;
        }
    }
}
