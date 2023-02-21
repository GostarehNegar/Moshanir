using System;

namespace GN.Library.Messaging.Messages
{
    public class LockRecordReply
    {
        public string Key { get; set; }
        public bool Acquired { get; set; }
        public DateTime ExpiresOn { get; set; }

    }
}
