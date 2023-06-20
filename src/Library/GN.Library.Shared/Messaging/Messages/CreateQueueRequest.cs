using GN.Library.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Messaging.Messages
{
    public class QueueSubscribeRequest
    {
        public string ConsummerId { get; set; }
        public string QueueName { get; set; }
        public string Subject { get; set; }
    }

    public class QueueSubscribeResponse
    {

    }
    public class CreateQueueRequest
    {
        public string Name { get; set; }
    }
    public class EnqueueRequest
    {
        public string QueueName { get; set; }
        public MessagePack Item { get; set; }
    }
    public class EnqueueReply
    {
        public string QueueName { get; set; }
        public MessagePack Message { get; set; }
    }
    public class CreateQueueReply
    {
        public string Name { get; set; }
        public MessagingQueueInformation Info { get; set; }
    }
    public class GetQueuesNamesRequest
    {

    }
    public class GetQueuesNamesReply
    {
        public string[] Queues { get; set; }
    }
}
