using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GN.Library.Messaging.Internals;
using GN.Library.Messaging.Transports;

namespace GN.Library.Messaging.Internals
{

    public interface IMessageBusEx : IMessageBus
    {
        CancellationToken CancellationToken { get; }
        Task Publish(IMessageContext message, CancellationToken cancellationToken = default);
        Task<IMessageBusSubscription> Subscribe(IMessageBusSubscription subscription);
        IMessageBusConfiguration Configuration { get; }
        Task SaveToStream(object[] messages, string stream);
        Task SaveToStream(ILogicalMessage x, bool skipPublish=false);
        Task Enqueue(IMessageContext context);
        void CancelRequest(IMessageContext request);
        IRequest CreateRequest(IMessageContext request, RequestOptions options=null);
        string EndpointName { get; }
        IServiceProvider ServiceProvider { get; }
        Task HandleReceive(IMessageTransport transport, object body, IDictionary<string, object> headers);
        bool IsConnected(int timeout);
        MessageBusOptions Options { get; }

        IMessageContext CreateMessageContext(MessagePack message);
    }
}
