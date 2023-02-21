using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Internals
{
    public class AssistantReceivedMessage
    {
        public string From { get; set; }
        public string Body { get; set; }

    }
    public class AssistantReceivedMessageReply
    {
        
        public string Body { get; set; }
        public int Accuracy { get; set; }

    }




}
