using GN.Library.Shared.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Chats
{
    public class ChatChannelVersionableEvent
    {
        public string Mode { get; set; }
        public string ChannelId { get; set; }
        public string EventType { get; set; }
        public long Version { get; set; }
        public DynamicEntity Payload { get; set; }
    }
}
