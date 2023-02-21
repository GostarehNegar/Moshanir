using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Internals
{
    public class QueryUserPresence
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }
    public class QueryUserPresenceResponse
    {
        public string Token { get; set; }
        public bool IsOnline { get; set; }
    }
}
