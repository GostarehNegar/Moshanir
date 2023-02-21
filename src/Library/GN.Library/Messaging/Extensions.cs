using GN.Library;
using GN.Library.Locks;
using GN.Library.Messaging;
using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN
{
    public static partial class MessagingExtensions
    {
        public static async Task<IMessageContext[]> WaitAll(this IRequest request, Func<IMessageContext[], bool> validator, int timeout=60*1000)
        {
            var result = new List<IMessageContext>();
            var source = new CancellationTokenSource(timeout);
            try
            {
                await request.WaitFor(ctx =>
                {
                    lock (result)
                    {
                        result.Add(ctx);
                    }

                    return validator(result.ToArray());
                }, source.Token);
            }
            catch (Exception err)
            {

            }



            return result.ToArray();
        }
        public static async Task<TRes> GetResponse<TReq, TRes>(this IMessageBus bus, TReq command, int miliSecondTimeout = 60 * 1000, bool Throw = true)
        {

            var request = bus.CreateMessage(command)
                .UseTopic(typeof(TReq))
                .CreateRequest();
            var reply = await request.WaitFor(x => true).TimeOutAfter(miliSecondTimeout);
            if (reply != null && reply.Message != null && reply.Message.TryCast<Exception>(out var _exp) && Throw)
            {
                throw _exp.Body;
            }
            return reply != null && reply.Message != null && reply.Message.TryCast<TRes>(out var _res) ? _res.Body : default(TRes);
        }
        public static Task Publish(this IMessageBus bus, object message, string topic = null)
        {
            if (message == null)
                throw new ArgumentException($"{nameof(message)} is NULL");
            var _topic = string.IsNullOrWhiteSpace(topic)
                    ? GN.Library.Messaging.Internals.MessageTopic.Create(message.GetType())
                    : GN.Library.Messaging.Internals.MessageTopic.Create(topic);
            return bus.CreateMessage(message).UseTopic(_topic.Subject).Publish();
        }


    }
}
