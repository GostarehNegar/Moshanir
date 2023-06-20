using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GN.Library.Messaging.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace GN.Library.Messaging
{
    //public interface IMessage
    //   {
    //	MessageHeader Header { get; }
    //	string Id { get; }
    //	string Subject { get; }
    //	byte[] Data { get; }
    //  object Body {get;}
    //	string TypeName { get; }
    //	string Stream { get; }
    //	long Sequence { get; }
    //  init(string id, string subject, byte [] data, Idictionary<string,string> header, string typeName, string stream, long? sequence)
    //   }
    internal interface IMessageContextInternal
    {

    }
    public interface IMessageContext 
    {
        IDictionary<string, object> Properties { get; }
        ILogicalMessage Message { get; }
        Task<IMessageContext> Publish(CancellationToken cancellationToken = default);
        Task Reply(object message);
        /// <summary>
        /// Acquires a message for execution.
        /// This is usefull in racing/ballancing scenariors when handlers compete and only one
        /// handler will be picked to execute the command.
        /// <para/>
        /// Command handlers will use this method to make sure that the command will be 
        /// executed only most by calling this method and executes the command only if the
        /// result is true: 
        /// <code>
        ///		if (await ctx.Acquire()) 
        ///		{ 
        ///			// run code
        ///		}
        /// </code>
        /// </summary>
        /// <param name="configure"></param>
        /// <param name="timeout">Millisecoonds to wait.</param>
        /// <param name="callBack">Call back to further inspect reply results.</param>
        /// <param name="token"></param>
        /// <returns> True if the message is acquired. </returns>
        Task<bool> Acquire(Action<AcquireMessageRequest> configure = null, int timeout = 60 * 1000, Action<AcquireMessageReply> callBack = null, CancellationToken token = default(CancellationToken));


        IRequest CreateRequest(Action<RequestOptions> configure = null);
        IMessageContext<T> Cast<T>();
        //IMessageContext<T> Convert<T>();
        bool TryCast<T>(out IMessageContext<T> result);
        bool TryCastEx(Type type, out IMessageContext result);
        //IMessageContext UseTopic(MessageSubject routing);
        IMessageContext UseTopic(string topic);
        IMessageContext UseTopic(string topic, string stream, long? version = null);
        //IMessageContext UseTopic(string topic);
        IMessageContext UseTopic(Type type);
        IMessageContext UseTopic(Type type, string stream, long? version = null);
        //IMessageContext UseTopic(Type type);
        IMessageContext Clone();
        IMessageHeader Headers { get; }
        IMessageBus Bus { get; }
        IServiceProvider ServiceProvider { get; }
        IServiceScope CreateScope();
        CancellationToken CancellationToken { get; }
        Task Ack();

    }
    public interface IMessageContext<T> : IMessageContext
    {
        new ILogicalMessage<T> Message { get; }


    }
}
