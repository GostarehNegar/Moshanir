namespace GN.Library.Messaging.Messages
{
    public class ReplayStreamReply
    {
        public MessagePack[] Events { get; set; }
        public long Position { get; set; }
        public long Remaining { get; set; }
    }
}
