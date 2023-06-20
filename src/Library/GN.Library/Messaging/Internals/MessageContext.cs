using GN.Library.Messaging.Messages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Internals
{

    public class MessageContext<T> : IMessageContext<T>, IMessageContextInternal
    {
        private MessageScope scope;

        private IMessageBusEx _bus;
        private ConcurrentDictionary<string, object> _properties = new ConcurrentDictionary<string, object>();
        public MessageContext(ILogicalMessage<T> message, IDictionary<string, object> props)
        {
            init(message, props, null);
        }
        public IServiceProvider ServiceProvider => this.scope?.ServiceProvider ?? this._bus?.Advanced().ServiceProvider;
        internal MessageContext(ILogicalMessage<T> message, IDictionary<string, object> props, IMessageBusEx eventBus)
        {
            init(message, props, eventBus);
        }
        internal MessageContext<T> init(ILogicalMessage<T> message, IDictionary<string, object> props, IMessageBusEx eventBus)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            this._bus = eventBus ?? MessagingExtensions.Services.GetEventBusEx();
            this.scope = new MessageScope(this._bus.ServiceProvider);
            this.Message = message;
            this._properties = new ConcurrentDictionary<string, object>(props ?? new Dictionary<string, object>());
            return this;
        }
        public ILogicalMessage<T> Message { get; protected set; }
        ILogicalMessage IMessageContext.Message => this.Message;

        public IDictionary<string, object> Properties => this._properties;

        public IMessageHeader Headers => this.Message?.Headers ?? new MessageHeader();

        public IMessageBus Bus => this._bus;

        public async Task<IMessageContext> Publish(CancellationToken cancellationToken = default)
        {
            await this._bus.Publish(this, cancellationToken);
            return this;
        }

        public async Task<bool> Acquire(Action<AcquireMessageRequest> configure = null, int timeout = 60 * 1000, Action<AcquireMessageReply> callBack = null, CancellationToken token = default(CancellationToken))
        {
            var key = "$acquired";
            if (this.Properties.GetValue<bool>(key))
            {
                return true;
            }
            var message = new AcquireMessageRequest { Load = 0 };
            configure?.Invoke(message);
            var context = new MessageContext<AcquireMessageRequest>(
               new LogicalMessage<AcquireMessageRequest>(
                   MessageTopic.Create(MessagingConstants.Topics.Acquire), message, null), null, this._bus);
            context.Message.To(this.Message.From());
            context.Message.InReplyTo(this.Message.MessageId);
            var res = await this._bus.CreateRequest(context).WaitFor(m => true, token)
                .TimeOutAfter(timeout);
            if (res != null && res.TryCast<AcquireMessageReply>(out var result))
            {
                callBack?.Invoke(result.Message.Body);
                this.Properties.SetValue<bool>(key, () => result.Message.Body.Acquired);
                return result.Message.Body.Acquired;
            }
            return false;
        }

        public Task Ack()
        {
            var queueMessage = this.QueueMessage();

            //return this.GetProperty<Func<Task>>("$ack", () => () => Task.CompletedTask).Invoke();
            if (queueMessage != null)
            {
                return queueMessage.Reply("ack");
            }
            return Task.CompletedTask;

            //return this.GetProperty<Func<Task>>("$ack", ()=> ()=> Task.CompletedTask).Invoke();
        }
        public Task Reply(object message)
        {
            if (message!=null && message is Exception __exp)
            {
                message = new Exception($"{__exp.GetBaseException().Message}");
            }
            var context = new MessageContext<object>(
                new LogicalMessage<object>(MessageTopic.Create(MessagingConstants.Topics.Reply), message, null), null, this._bus);
            context.Message.Headers.AddFlag(MessageFlags.Reply);
            context.Message.To(this.Message.From());
            context.Message.InReplyTo(this.Message.MessageId);
            if ( message !=null && message is Exception _exp)
            {
                context.Message.Headers.StatusCode(1);
                context.Message.Headers.ErrorMessage(_exp.GetBaseException().Message);

            }
            return this._bus.Publish(context);

        }

        public IMessageContext<TO> Cast<TO>()
        {
            return this.TryCast<TO>(out var result)
                ? result
                : null;

        }
        //public IMessageContext<TO> Convert<TO>()
        //{
        //    return this.TryConver<TO>(out var result)
        //        ? result
        //        : null;

        //}
        public IMessageContext Clone()
        {
            return new MessageContext<T>(this.Message, this.Properties, this._bus);
        }
        public bool TryCast<TO>(out IMessageContext<TO> result)
        {
            if (this.Message != null && this.Message.TryCast<TO>(out var m))
            {
                result = new MessageContext<TO>(m, this.Properties, this._bus);
                return true;
            }
            result = null;
            return false;
        }
        public bool TryCastEx(Type type, out IMessageContext result)
        {
            var p = new object[] { null };
            var res = ((bool)this.GetType()
                .GetMethod(nameof(this.TryCast))
                .MakeGenericMethod(type)
                .Invoke(this, p));
            result = p[0] as IMessageContext;
            return res;
        }


        //public bool TryConver<TO>(out IMessageContext<TO> result)
        //{
        //    if (this.Message != null && this.Message.TryConvert<TO>(out var m))
        //    {
        //        result = new MessageContext<TO>(m, this.Properties, this._bus);
        //        return true;
        //    }
        //    result = null;
        //    return false;
        //}

        //public Task<IRequest> Send_Deprecated(Func<IMessageContext, bool> validator = null, CancellationToken cancellationToken = default)
        //{
        //    return this._bus.Send_Deprecated(this, validator, cancellationToken);
        //}

        public IMessageContext UseTopic(MessageTopic routing)
        {
            this.Message.SetTopic(routing);
            return this;
        }
        public IMessageContext UseTopic(string subject)
        {
            return UseTopic(MessageTopic.Create(subject));
        }

        public IMessageContext UseTopic(string subject, string stream, long? version = null)
        {
            return UseTopic(MessageTopic.Create(subject, stream, version));
        }
        public IMessageContext UseTopic(Type type)
        {
            return UseTopic(MessageTopicHelper.GetTopicByType(type));
        }
        public IMessageContext UseTopic(Type type, string stream, long? version = null)
        {
            return UseTopic(MessageTopicHelper.GetTopicByType(type), stream, version);
        }

        public IRequest CreateRequest(Action<RequestOptions> configure = null)
        {
            var options = new RequestOptions();
            configure?.Invoke(options);
            return this._bus.CreateRequest(this, options);
        }

        public IServiceScope CreateScope()
        {
            this.scope = this.scope ?? new MessageScope(this.ServiceProvider);
            return this.scope.CreateScope();

        }
        public CancellationToken CancellationToken => this.Bus?.Advanced().CancellationToken ?? default;
    }
}


