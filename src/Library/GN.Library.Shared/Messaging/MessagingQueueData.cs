using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Messaging
{
    public class MessagingQueueData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long ItemsCount { get; set; }
        public MessagingQueueData()
        {
            Id = $"Queue {Guid.NewGuid()}";
        }
    }
    public class MessagingQueueInformation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string EndpointName { get; set; }
        
        public MessagingQueueInformation()
        {
            Id = $"Queue {Guid.NewGuid()}";
        }
    }
}
