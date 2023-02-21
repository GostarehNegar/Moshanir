using GN.Library.Pipelines;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Internals
{
    class MessageBusSubscription : IMessageBusSubscription
    {

        private SubscriptionProperties _properties = new SubscriptionProperties();
        public MessageBusSubscription()
        {
            this.Id = Guid.NewGuid();
        }
        public long? LastMessageVersion { get; set; }
        public Guid Id { get; private set; }
        public ISubscriptionProperties Properties => this._properties;
        public SubscriptionTopic Topic { get; private set; }

        private Func<IMessageContext, Task> handler;

        private Func<Func<IMessageContext, Task>> handlerConstructor;
        public bool IsShared { get; set; }

        public bool IsDeactive { get; set; }
        public bool NoVersionControl { get; set; }

        /// <summary>
        /// When specified, the subscription is relay that receives
        /// all messages that are To that endpoint. 
        /// That means this subsciption matches for messages
        /// when To==Relay regardless of the subject.
        /// 
        /// </summary>
        public string RelayEndpoint { get; set; }

        private bool VersionMatch(IMessageContext context)
        {
            // return true;
            if (NoVersionControl)
                return true;
            var version = context.Version();
            if (!version.HasValue || version.Value < 0)
                return true;
            if (!this.LastMessageVersion.HasValue || this.LastMessageVersion.Value < version.Value)
            {
                this.LastMessageVersion = version;
                return true;
            }
            //throw new Exception("Unexpcted Version")
            return false;
        }
        public IMessageBusSubscription AddHandler(Func<Func<IMessageContext, Task>> handler)
        {
            this.handlerConstructor = handler;
            return this;
        }


        public IMessageBusSubscription AddHandler(Func<IMessageContext, Task> handler)
        {

            this.handler = ctx =>
            {
                if (1 == 1)// || VersionMatch(ctx))
                {
                    return handler == null ? Task.CompletedTask : handler?.Invoke(ctx);
                }
                else
                {
                    return Task.CompletedTask;
                }
            };
            this.handlerConstructor = () => this.handler;
            return this;
        }

        private Func<IMessageContext, Task> GetHandler()
        {
            return this.handlerConstructor?.Invoke();
        }

        public IMessageBusSubscription UseTopic(SubscriptionTopic topic)
        {
            this.Topic = topic;
            return this;
        }
        public IMessageBusSubscription AddHandler<T>(Func<IMessageContext<T>, Task> handler)
        {
            this.handler = ctx =>
            {
                //if (1 == 1 && !VersionMatch(ctx))
                //    return Task.CompletedTask;
                return ctx.TryCast<T>(out var o) && o != null
                ? handler == null ? Task.CompletedTask : handler(o)
                : Task.CompletedTask;
            };
            this.handlerConstructor = () => this.handler;
            return this;
        }
        public bool Matches(IMessageContext message)
        {

            if (!string.IsNullOrWhiteSpace(this.RelayEndpoint) && message?.Message?.To() == this.RelayEndpoint)
            {
                return true;
            }
            if (this.Topic.Subject == LibraryConstants.Subjects.StarStar)
                return true;
            if (message?.Message?.ReplayFor() != null && message?.Message?.ReplayFor() != this.Id.ToString())
            {
                /// if the message is being replayed for someone else...
                /// 
                return false;
            }
            return this.Topic != null && this.Topic.Matches(message?.Message);
        }

        public Task Handle(IMessageContext message)
        {
            if (message == null || (this.Topic.Subject != LibraryConstants.Subjects.StarStar && !VersionMatch(message)))
            {
                return Task.CompletedTask;
            }
            return this.GetHandler() == null ? Task.CompletedTask : this.GetHandler()(message);
        }
        public Task Handle<T>(IMessageContext<T> message)
        {
            return this.GetHandler() == null ? Task.CompletedTask : this.GetHandler()(message);
        }


        public void Deactivate()
        {
            this.IsDeactive = true;
        }



    }


}
