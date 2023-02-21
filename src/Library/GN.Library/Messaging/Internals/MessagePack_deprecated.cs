using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Internals
{

    public class MessagePack_deprecated
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string Stream { get; set; }
        public long Version { get; set; }
        public string TypeName { get; set; }
        public string Payload { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public MessagePack_deprecated()
        {

        }
    }
}
