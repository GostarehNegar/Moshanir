using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Natilus.Messaging.Internals
{
    public class NatilusMessageContext : INatilusMessageContext
    {
        private readonly NatilusBus bus;

        public NatilusMessageContext(INatilusBus bus, NatilusMessage message)
        {
            this.bus = bus as NatilusBus;
            this.Message = message;
        }
        public NatilusMessage Message { get; private set; }

        public PublishStrategy Strategy { get; set; }
        public bool SkipLegacyBus { get; private set; }

        public Task Publish(CancellationToken cancellationToken = default)
        {
            return this.bus.NatilusPublish(this,cancellationToken);
        }

        public INatilusMessageContext WithSkipLegacyBus()
        {
            this.SkipLegacyBus = true;
            return this;
        }

        public INatilusMessageContext WithStrategy(PublishStrategy strategy)
        {
            this.Strategy = strategy;
            return this;
        }
    }
}
