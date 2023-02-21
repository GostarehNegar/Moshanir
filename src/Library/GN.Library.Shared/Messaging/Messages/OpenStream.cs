using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Messages
{
    public class OpenStream
    {
        public string TopicFilter { get; set; }
        public string Stream { get; set; }
        //public string StreamId { get; set; }
        public long? Position { get; set; }
        public string Id { get; set; }

        public int? ChunkSize { get; set; }

        public bool Mode { get; set; }
    }
}
