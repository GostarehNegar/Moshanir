using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Natilus.Messaging
{
    public delegate void MessageHandler(INatilusMessageContext context);
    public delegate Task MessageHandlerAsync(INatilusMessageContext context);
    public interface INatilusSubscriptionBuilder
    {
        Task<INatilusSubscription> Build();
        INatilusSubscriptionBuilder WithHandler(MessageHandler handler);
        INatilusSubscriptionBuilder WithAsyncHandler(MessageHandlerAsync handler);
        INatilusSubscriptionBuilder WithSubject(string subject);
    }
}
