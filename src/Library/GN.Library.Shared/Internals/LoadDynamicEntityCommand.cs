using GN.Library.Shared.Chats;
using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Internals
{
    public class LoadDynamicEntityCommand
    {
        public string LogicalName { get; set; }
        public string Id { get; set; }
    }
    public class LoadDynamicEntityReply
    {
        public DynamicEntity Result { get; set; }
    }
}
