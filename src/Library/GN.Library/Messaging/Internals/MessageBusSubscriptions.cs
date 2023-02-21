using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GN.Library.Messaging.Internals
{
    class MessageBusSubscriptions
    {
        private ConcurrentDictionary<Guid, IMessageBusSubscription> _subscriptions = new ConcurrentDictionary<Guid, IMessageBusSubscription>();

        public IMessageBusSubscription Add(IMessageBusSubscription sub)
        {
            this._subscriptions.AddOrUpdate(sub.Id, sub, (id, s) => sub);
            return sub;
        }

        public IMessageBusSubscription Add(Action<IMessageBusSubscription> configure)
        {
            var result = new MessageBusSubscription();
            configure?.Invoke(result);
            return Add(result);
            this._subscriptions.AddOrUpdate(result.Id, result, (id, s) => result);
            return result;

        }

        public IEnumerable<IMessageBusSubscription> Query(IMessageContext ctx)
        {
            var ret = this._subscriptions.Values.Where(x => x.Matches(ctx) && !x.IsDeactive).ToArray();
            return ret;
        }

    }
}
