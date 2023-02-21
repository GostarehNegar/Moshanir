using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Hosting;

namespace GN.Library.Messaging.Hubs
{
    public class MessageReceivedEventArgs
    {
        public MessageContext Message { get; private set; }
        public CancellationToken CancellationToken { get; private set; }
        public object Sender { get; private set; }

        public MessageReceivedEventArgs(object sender, MessageContext message, CancellationToken cancellationToken)
        {
            this.Sender = sender;
            this.Message = message;
            this.CancellationToken = cancellationToken;
        }
    }
    public delegate Task<bool> OnMessageReceived(MessageReceivedEventArgs message);

    public interface IMessageHub : IHostedService
    {
        string Endpoint { get; }
        IMessageHub Configure(bool reset = true, string endPointName = null);
        Task Subscribe(BusSubscription subscription);
        Task Publish(MessageContext message, BusSubscription subscription, bool toServer = true, bool toCleints = true);
        string Login(string userName, string password, string role);
        event OnMessageReceived OnMessageReceived;
        bool IsActive { get; }
        string Id { get; }
        string ServerEndpoint { get; }
        Task<string> GetServerEndpoint();
        Task<bool> Publish(MessageContext message, string endpoint, bool publishToServer);

    }
}
