using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Messages
{
    public class SaveEventToStream
    {
        public object[] Events { get; set; }
        public string Stream { get; set; }
        //public string StreamId { get; set; }
        public bool SkipPublish { get; set; }


    }
}
