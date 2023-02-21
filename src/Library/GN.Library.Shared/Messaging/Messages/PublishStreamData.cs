using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Messages
{
    public class PublishStreamData
    {
        public MessagePack[] Events { get; set; }
        public string Stream { get; set; }


    }
}
