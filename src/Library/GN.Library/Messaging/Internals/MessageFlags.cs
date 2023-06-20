using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Internals
{
    [Flags]
    public enum MessageFlags
    {
        Publish = 0,
        Request = 1,
        Reply = 2,
        Error = 2 ^ 2,
        Ack = 2 ^ 3,
        QueuedMessage = 2 ^ 4,

    }
}
