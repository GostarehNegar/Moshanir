using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Messages
{

    public class LockRecordCommand
    {
        public string Key { get; set; }
        public int Timeout { get; set; }
        public int Expiration { get; set; }
    }
}
