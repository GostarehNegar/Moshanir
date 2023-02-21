using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Natilus.Messaging
{
    public enum PublishStrategy
    {
        Nats,
        JetStream,
        Auto
    }
    public interface INatilusMessageContext
    {
        Task Publish(CancellationToken cancellationToken=default);
        NatilusMessage Message { get; }
        PublishStrategy Strategy { get; }
        bool SkipLegacyBus { get; }
        INatilusMessageContext WithStrategy(PublishStrategy strategy);
        INatilusMessageContext WithSkipLegacyBus();


    }
}
