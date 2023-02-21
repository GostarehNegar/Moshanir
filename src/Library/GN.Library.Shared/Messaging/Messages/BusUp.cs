using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Messages
{
    public class BusUp
    {
        public class HandlerInfo
        {
            public string MessageType { get; set; }
            public string Topic { get; set; }
            public string Type { get; set; }
        }

        public Uri Address { get; set; }
        public HandlerInfo[] Handlers { get; set; }
    }

}
