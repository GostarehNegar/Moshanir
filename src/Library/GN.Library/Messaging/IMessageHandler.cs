using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging
{
    public interface IMessageHandlerConfigurator
    {
        void Configure(ISubscriptionBuilder subscription);
    }

    public interface IMessageHandler
    {

    }
    public interface IMessageHandler<T>: IMessageHandler
    {
        Task Handle(IMessageContext<T> context);
    }
}
