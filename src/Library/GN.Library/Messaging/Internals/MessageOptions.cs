using GN.Library.Messaging.Transports;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Internals
{
    public class MessageOptions
    {
        public string QueueName { get; private set; }
        public Func<IMessageTransport, bool> TransportSelector;

        public bool ByPassDuplicateValidation { get; private set; }
        public bool TransportMatch(IMessageTransport transport)
        {
            return this.TransportSelector == null || this.TransportSelector(transport);
        }
        public bool LocalOnly { get; set; }

        public static MessageOptions GetDefault()
        {
            return new MessageOptions();
        }
        public MessageOptions WithQueue(string queueName)
        {
            this.QueueName = queueName;
            return this;
        }
        public MessageOptions WithBypassDuplicateValidations(bool value)
        {
            this.ByPassDuplicateValidation = value;
            return this;
        }

    }
}
