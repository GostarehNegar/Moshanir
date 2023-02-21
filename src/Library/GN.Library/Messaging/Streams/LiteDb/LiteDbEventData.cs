using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Streams.LiteDb
{
    public class LiteDbEventData
    {
        public long Id { get; set; }
        public string MessageId { get; set; }
        public string TypeName { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Name { get; set; }
        public string Payload { get; set; }
        public DateTime Timestamp { get; set; }

        public MessagePack ToMessagePack()
        {
            return new MessagePack
            {
                Id = this.MessageId,
                Payload = this.Payload,
                Subject = this.Name,
                TypeName = this.TypeName,
                Headers = this.Headers,
                Version = this.Id,
                Timestamp = this.Timestamp

            };
        }
        public static LiteDbEventData FromMessagePack(MessagePack pack)
        {
            return new LiteDbEventData
            {
                MessageId = pack.Id,
                Payload = pack.Payload,
                Headers = pack.Headers,
                TypeName = pack.TypeName,
                Name = pack.Subject,
                Timestamp = pack.Timestamp
            };
        }
    }
}
