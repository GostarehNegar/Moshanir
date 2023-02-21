using GN.Library.Shared.Chats;
using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace GN.Library.Shared.Internals
{
    public class ResolveIdentityCommand
    {
        public DynamicEntity User { get; set; }
    }
    public class ResolveIdentityReply
    {
        public DynamicEntity User { get; set; }
        //public ClaimsIdentity Identity { get; set; }
        public string UserId { get; set; }
    }
}
