using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GN.Library.Shared.Entities;

namespace GN.Library.Shared.Chats
{
    public class ChatMessageEntity : XrmDynamicEntity
    {
        public static new class Schema
        {
            public const string LogicalName = "message";
        }
        public ChatMessageEntity()
        {
            this.LogicalName = Schema.LogicalName;
        }
        public string Author { get => this.GetAttributeValue("Author"); set => this.SetAttributeValue("Author", value); }
        public string Message { get => this.GetAttributeValue("Message"); set => this.SetAttributeValue("Message", value); }
    }

}
