using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Chats
{
    public class CommandPack
    {
        public string PayloadTypeName { get; set; }
        public string Payload { get; set; }
        public static CommandPack FromObject(object obj)
        {
            return new CommandPack()
            {
                Payload = Newtonsoft.Json.JsonConvert.SerializeObject(obj),
                PayloadTypeName = obj.GetType().AssemblyQualifiedName

            };
        }
        public object GetBody()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(this.Payload, Type.GetType(this.PayloadTypeName));
        }
    }
}
