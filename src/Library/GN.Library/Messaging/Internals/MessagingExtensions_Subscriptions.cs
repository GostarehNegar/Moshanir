using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging
{
    public static partial class MessagingExtensions
    {


        public static string Endpoint(this ISubscriptionProperties subs, string endpoint = null)
        {
            if (endpoint != null)
            {
                subs["original-endpoint"] = endpoint;
            }
            return subs.TryGetValue("original-endpoint", out var _res) ? _res : null;
        }
        public static string RemoteId(this ISubscriptionProperties props, string remoteId = null)
        {
            if (remoteId != null)
            {
                props["remote-id"] = remoteId;
            }
            return props.TryGetValue("remote-id", out var _res) ? _res : null;

        }

        public static ISubscriptionBuilder WithRemoteId(this ISubscriptionBuilder builder, string remoteId)
        {
            builder.Properties.RemoteId(remoteId);
            return builder;
        }

        public static IMessageContext WithQueue(this IMessageContext context, string queueName)
        {
            context.Options().WithQueue(queueName);
            return context;
        }
        public static IMessageContext WithOptions(this IMessageContext context, Action<MessageOptions> configure)
        {
            configure?.Invoke(context.Options());
            return context;
        }

        public static T GetProperty<T>(this IMessageContext context, string key, Func<T> constructor = null)
        {
            key = key ?? typeof(T).FullName;
            if (context.Properties.TryGetValue(key, out var _res)
                && _res != null
                && typeof(T).IsAssignableFrom(_res.GetType()))
            {
                return (T)(object)_res;
            }
            if (constructor != null)
            {
                var val = constructor();
                context.Properties[key] = val;
                return val;
            }
            return default;
        }
        public static void SetProperty<T>(this IMessageContext context, string key, T value)
        {
            key = key ?? typeof(T).FullName;
            context.Properties[key] = value;
        }
        public static void RemoveProperty<T>(this IMessageContext context, string key)
        {
            key = key ?? typeof(T).FullName;
            context.Properties.Remove(key);
        }

    }
}
