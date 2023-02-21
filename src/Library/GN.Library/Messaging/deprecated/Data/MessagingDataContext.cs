using GN.Library.Data;
using GN.Library.Data.Deprecated;
using GN.Library.Data.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Data
{
    public interface IMessagingDataContext : IDisposable
    {
        IMessageRepository Messages { get; }
        ISubscriptionRepository Subscriptions { get; }
        IAcknowledgeRepository Acknowledgements { get; }
        IEndPointRepository EndPoints { get; }
        IMessageQueueRepository MessageQueue { get; }
        void ClearAll();
        Task CleanUp(bool force = false);
        Task<IEnumerable<EndPointData>> GetMatchingEndpoints(MessageContext message);

    }
    class MessagingDataContext : DocumentStore, IMessagingDataContext
    {
        private IAcknowledgeRepository acknowledge;
        private IMessageRepository messages;
        private ISubscriptionRepository subscriptions;
        private IEndPointRepository endPoints;
        private IMessageQueueRepository messageQueue;


        public MessagingDataContext(MessagingConfig cfg)
        {
            this.connectionString = new DocumentStoreConnectionString
            {
                FileName = MessagingConstants.Instance.GetMessagingDbFileName(cfg.EndPointName)
            };
        }

        public IMessageRepository Messages => GetMessages();
        public ISubscriptionRepository Subscriptions => GetSubscriptions();
        public IAcknowledgeRepository Acknowledgements => GetAcknowledge();
        public IMessageQueueRepository MessageQueue => GetMessageQueue();
        public IEndPointRepository EndPoints => GetEndPoints();
        public IAcknowledgeRepository GetAcknowledge(bool refresh = false)
        {
            if (this.acknowledge == null || refresh)
            {
                this.acknowledge = new AcknowledgeRepository(this.GetDatabase());
            }
            return this.acknowledge;
        }
        public IMessageRepository GetMessages(bool refresh = false)
        {
            if (this.messages == null || refresh)
            {
                this.messages = new MessageRepository(this.GetDatabase());
            }
            return this.messages;
        }


        public ISubscriptionRepository GetSubscriptions(bool refresh = false)
        {
            if (this.subscriptions == null || refresh)
            {
                this.subscriptions = new SubscriptionRepository(this.GetDatabase());
            }
            return this.subscriptions;
        }

        public IEndPointRepository GetEndPoints(bool refresh = false)
        {
            if (this.endPoints == null || refresh)
            {
                this.endPoints = new EndPointRepository(this.GetDatabase());
            }
            return this.endPoints;

        }

        public IMessageQueueRepository GetMessageQueue(bool refersh = false)
        {
            if (this.messageQueue == null || refersh)
            {
                this.messageQueue = new MessageQueueRepository(this.GetDatabase());
            }
            return this.messageQueue;

        }
        void IDisposable.Dispose()
        {
            base.Dispose();
            this.subscriptions = null;
            this.acknowledge = null;
            this.messages = null;
            this.endPoints = null;
            this.db?.Dispose();
            this.db = null;

        }

        public void ClearAll()
        {
            this.Acknowledgements.Clear();
            this.EndPoints.DeleteAll();
            this.Subscriptions.Clear();
            this.Messages.DeletAll();
            this.MessageQueue.DeleteAll();
        }


        public async Task<IEnumerable<EndPointData>> GetMatchingEndpoints(MessageContext message)
        {
            return (string.IsNullOrWhiteSpace(message.To))

                ? (await this.GetSubscriptions().GetSubscriptions(message))
                    .DistinctBy(x => x.Endpoint)
                    .Select(x => this.EndPoints.GetEndPoint(x.Endpoint, true))
                : this.EndPoints.GetCachedValues().Where(x => x.Name == message.To);
        }

        public Task CleanUp(bool force = false)
        {
            return Task.WhenAll(
                this.MessageQueue.CleanUp(force),
                this.EndPoints.CleanUp(force));
        }
    }








}
