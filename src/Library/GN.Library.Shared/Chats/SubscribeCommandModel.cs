using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Chats
{
    public class SubscribeCommandModel
    {
        public string ChannelId { get; set; }
    }
    public class SubscribeResultModel
    {
        public bool Success { get; set; }
    }
}
