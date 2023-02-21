using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Internals
{
    public class ChangeDynamicEntityStatusCommand
    {
        public string LogicalName { get; set; }
        public string Id { get; set; }
        public int StatusCode { get; set; }
        public int StateCode { get; set; }


    }
    public class ChangeDynamicEntityStatusReply
    {
        public bool DidChange { get; set; }
    }
}
