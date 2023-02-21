using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class MessageHandlerAttribute : Attribute
    {
        public string Topic { get; set; }
    }
}
