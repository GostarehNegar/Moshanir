using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GN.Library.Messaging.Internals;

namespace GN.Library.Messaging.Internals
{
    public interface ISubscriptionProperties : IDictionary<string, string> { }
    public class SubscriptionProperties : ConcurrentDictionary<string, string>, ISubscriptionProperties
    {
        public SubscriptionProperties() : base()
        {

        }
        public SubscriptionProperties(IEnumerable<KeyValuePair<string, string>> items) : base(items ?? new Dictionary<string, string>())
        {

        }

    }
    public interface IMessageBusSubscription : IDisposable
    {
        ISubscriptionProperties Properties { get; }
        Guid Id { get; }
        string QueueName { get; }
        IMessageBusSubscription UseTopic(SubscriptionTopic topic);
        IMessageBusSubscription AddHandler(Func<IMessageContext, Task> handler);
        IMessageBusSubscription AddHandler<T>(Func<IMessageContext<T>, Task> handler);
        IMessageBusSubscription AddHandler(Func<Func<IMessageContext, Task>> handler);
        SubscriptionTopic Topic { get; }
        bool Matches(IMessageContext message);

        Task Handle<T>(IMessageContext<T> message);
        Task Handle(IMessageContext message);
        bool IsDeactive { get; }
        void Deactivate();
        string RelayEndpoint { get; set; }
    }
}
