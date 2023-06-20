using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging
{
    //public interface IMessagePack_DEP
    //{
    //    long Version { get; }
    //    string Payload { get; }
    //    string Subject { get; }
    //    string Id { get; }
    //    string Stream { get; }

    //    string TypeName { get; }
    //    Dictionary<string,string> Headers { get; }

    //    DateTime Timestamp { get; }

    //}
    public class QueueMessage
    {
        public MessagePack Pack { get; set; }
    }
    public class MessagePack
    {
        public MessagePack()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Headers = new Dictionary<string, string>();
        }
        public MessagePack(long? version, string payload, string name, DateTime timestamp)
        {
            Version = version;
            Subject = name;
            Payload = payload;
            Timestamp = timestamp;
        }

        public string Id { get; set; }
        public string Stream { get; set; }
        public long? Version { get; set; }

        public string Payload { get; set; }

        public string Subject { get; set; }
        public string TypeName { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public DateTime Timestamp { get; set; }

        public string GetHeaderValue(string key)
        {
            this.Headers = this.Headers ?? new Dictionary<string, string>();
            return this.Headers.TryGetValue(key, out var res)
                ? res
                : null;
        }
        public long GetVersion()
        {
            return Version ?? -1;
        }
    }
}
