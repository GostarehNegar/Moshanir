using System;

namespace GN.Library.Messaging.Messages
{
    public class ReplayStreamCommand
    {
        public string Stream { get; set; }
        //public string StreamId { get; set; }
        public long? Position { get; set; }
        public Guid Id { get; set; }

        public int? ChunkSize { get; set; }

    }
}
