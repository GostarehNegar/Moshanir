using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client;
using NATS.Client.JetStream;
using GN.Library.Natilus.Internals;
using GN.Library.Messaging.Internals;

namespace GN.Library.Natilus.Messaging.Internals
{
    class NatilusBus : MessageBus, INatilusBusEx, IHostedService
    {
        private readonly ILogger<NatilusBus> logger;
        private readonly IServiceProvider serviceProvider;
        private INatilusConnectionProvider connectionProvider;
        private JetStreamHelper streamHelper;
        public NatilusOptions NatilusOptions { get; private set; }

        public NatilusBus(IServiceProvider serviceProvider) : base(serviceProvider, null)
        {
            this.logger = serviceProvider.GetServiceEx<ILogger<NatilusBus>>();
            this.serviceProvider = serviceProvider;
            this.connectionProvider = this.serviceProvider.GetServiceEx<INatilusConnectionProvider>();
            this.NatilusOptions = this.serviceProvider.GetServiceEx<NatilusOptions>();
        }
        public override INatilusMessageContext CreateNatilusMessage(string subject, object message)
        {
            var result = new NatilusMessageContext(this, new NatilusMessage(subject, message, null, null));
            return result;
        }
        private Task<IConnection> GetConnection()
        {
            return this.connectionProvider.GetConnectionAsync("bus");
        }
        private async Task<IJetStream> GetJetStream()
        {
            return (await this.GetConnection()).CreateJetStreamContext();
        }
        private async Task<JetStreamHelper> GetJetStreamHelper(bool refresh = false)
        {
            if (this.streamHelper == null || refresh)
            {
                this.streamHelper = new JetStreamHelper(await this.GetConnection());
            }
            return this.streamHelper;
        }
        public async Task NatilusPublish(NatilusMessageContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(context.Message.From()))
                {
                    context.Message.From(this.EndpointName);
                }
                var msg = context.Message.ToMsg();
                var helper = await this.GetJetStreamHelper();
                if (!context.SkipLegacyBus)
                {
                    var ctx = new MessageContext<object>(context.Message.GetLogicalMessage(), null, this);
                    await this.DoPublish(ctx, cancellationToken);
                }
                var strategy = context.Strategy;
                if (strategy == PublishStrategy.Auto)
                {
                    strategy = helper.ConnectionSupportsJetStream() && helper.SubjectHasStream(msg.Subject)
                        ? PublishStrategy.JetStream
                        : PublishStrategy.Nats;
                }
                switch (strategy)
                {
                    case PublishStrategy.Auto:
                        {
                            try
                            {
                                await (await this.GetJetStream())
                                    .PublishAsync(msg);
                            }
                            catch (Exception err)
                            {

                            }
                            (await this.GetConnection())
                                .Publish(msg);
                        }
                        break;
                    case PublishStrategy.JetStream:
                        {
                            await (await this.GetJetStream())
                                .PublishAsync(context.Message.ToMsg());
                        }
                        break;
                    default:
                        {
                            (await this.GetConnection())
                                .Publish(context.Message.ToMsg());
                        }
                        break;
                }
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to publish message. {err.Message}");
                throw;
            }
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
            var conn = await this.GetConnection();
            if (conn != null)
            {
                this.logger.LogInformation(
                    $"Natilus Bus Successfully Started. NATS Url:'{conn.ConnectedUrl}'");
            }
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {

            return base.StopAsync(cancellationToken);
        }
        public override INatilusSubscriptionBuilder CreateNatilusSubscription(string subject)
        {
            return new NatilusSubscription(this).WithSubject(subject);
        }
        internal async Task NatilusSubscribe(NatilusSubscription subscription)
        {
            await subscription.DoSubscribe(this, await this.GetConnection(), this.Serializer());
        }
        public INatilusSerializer Serializer()
        {
            return serviceProvider.GetServiceEx<INatilusSerializer>() ?? NatilusSerializer.Default;
        }
    }
}
