using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Messages
{
    public class AcquireMessageRequest
    {
        public int Load { get; set; }
        public int RacingWait { get; set; }
    }
}
