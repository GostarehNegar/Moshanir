using GN.Library.Pipelines;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Internals
{
	public interface ISubscriptionBuilder
	{
		ISubscriptionProperties Properties { get; }
		ISubscriptionBuilder UseTopic(SubscriptionTopic topic);
		ISubscriptionBuilder UseTopic(string subject, string stream=null,  long? fromVersion=null, long? toVersion=null);
		ISubscriptionBuilder UseTopic(Type type, string stream = null,  long? fromVersion = null, long? toVersion = null);
		ISubscriptionBuilder UseHandler(Func<IMessageContext, Task> handler);
		ISubscriptionBuilder UseHandler<T>(Func<IMessageContext<T>, Task> handler);
		ISubscriptionBuilder WithEnpoint(string endpoint);
		ISubscriptionBuilder WithRelay(string endpoint);
		
		ISubscriptionBuilder WithNoVersionControl();


		Task<IMessageBusSubscription> Subscribe();
		IMessagingServices ServiceProvider { get; }
		IMessageBusConfiguration Configuration { get; }

		
	}

	class SubscriptionBuilder : ISubscriptionBuilder
	{
		private IMessageBusEx _bus;
		private MessageBusSubscription _subscription;
		private bool subscribed;
		
		

		public SubscriptionBuilder(IMessageBusEx bus)
		{
			this._bus = bus;
			this._subscription = new MessageBusSubscription();
			this._subscription.Properties.Endpoint(this._bus.Advanced().EndpointName);
		}

		public IMessagingServices ServiceProvider => this._bus.Configuration.ServiceProvider;


		public IMessageBusConfiguration Configuration => this._bus.Configuration;

		public ISubscriptionProperties Properties => this._subscription.Properties;

		public Task<IMessageBusSubscription> Subscribe()
		{
			var result = this.subscribed
				? Task.FromResult<IMessageBusSubscription>(this._subscription)
				: this._bus.Subscribe(this._subscription);
			this.subscribed = true;
			return result;
		}
		public ISubscriptionBuilder UseHandler(Func<IMessageContext, Task> handler)
		{
			//if (handler != null)
			{
				this._subscription.AddHandler(handler);
			}
			return this;
		}
		public ISubscriptionBuilder UseHandler<T>(Func<IMessageContext<T>, Task> handler)
		{
			if (this._subscription.Topic == null)
            {
				this.UseTopic(typeof(T));
            }
			this._subscription.AddHandler<T>(handler);
			return this;
		}


		public ISubscriptionBuilder UseTopic(SubscriptionTopic topic)
		{
			this._subscription.UseTopic(topic);
			
			return this;
		}


		public ISubscriptionBuilder UseTopic(string topic, string stream = null,  long? fromVersion = null, long? toVersion = null)
		{
			this._subscription.UseTopic(
				SubscriptionTopic.Create(topic, stream,fromVersion,toVersion));

			return this;
		}

		public ISubscriptionBuilder UseTopic(Type type, string stream = null,  long? fromVersion = null, long? toVersion = null)
		{
			this._subscription.UseTopic(
				SubscriptionTopic.Create(type, stream,  fromVersion, toVersion));
			return this;
		}
		public ISubscriptionBuilder WithEnpoint(string endpoint)
        {
			if (!string.IsNullOrWhiteSpace(endpoint))
            {
				this._subscription.Properties.Endpoint(endpoint);
            }
			return this;
        }

        public ISubscriptionBuilder WithNoVersionControl()
        {
			this._subscription.NoVersionControl = true;
			return this;
        }

        public ISubscriptionBuilder WithRelay(string endpoint)
        {
			this._subscription.RelayEndpoint = endpoint;
			return this;
        }
    }
}
