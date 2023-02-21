using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Internals
{
    public class SendChatMessageCommand
    {
        public string To { get; set; }
        public string Body { get; set; }
    }
    public class SendChatMessageReply
    {
        public bool Success { get; set; }
    }
    public class SendXmppChatMessageCommand : SendChatMessageCommand
    { }
    public class SendXmppChatMessageReply : SendChatMessageReply
    { }
}
