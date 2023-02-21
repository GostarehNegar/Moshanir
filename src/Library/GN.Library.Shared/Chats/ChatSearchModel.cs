using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Chats
{
    public class ChatSearchModel
    {
        public string SearchText { get; set; }
    }
    public class ChatSearchResultModel
    {
        public DynamicEntity[] Results { get; set; }

    }
}
