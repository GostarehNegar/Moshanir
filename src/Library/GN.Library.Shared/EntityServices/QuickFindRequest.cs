using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.EntityServices
{
    public class QuickFindRequest
    {
        public string[] Entitites { get; set; }
        public string SearchText { get; set; }
    }
    public class QuickFindReply
    {
        public DynamicEntity[] Result { get; set; }
    }
}
