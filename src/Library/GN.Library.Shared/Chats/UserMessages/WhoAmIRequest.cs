using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Chats.UserMessages
{
    public class WhoAmIRequest
    {
        public string Tag { get; set; }
    }
    public class WhoAmIResponse
    {
        public string DisplayName { get; set; }
    }
}
