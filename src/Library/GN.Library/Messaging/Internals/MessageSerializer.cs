using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace GN.Library.Messaging.Internals
{
    public interface IMessageSerializer
    {
        bool TrySerialize(object @object, out string result);
        bool TryDeserialize(string data, Type type, out object result);
    }
    class MessageSerializer : IMessageSerializer
    {
        static JsonSerializerSettings settings = new JsonSerializerSettings();
        IMessageSerializer Current => AppHost.GetService<IMessageSerializer>() ?? new MessageSerializer();

        public bool TryDeserialize(string data, Type type, out object result)
        {
            try
            {
                result = JsonConvert.DeserializeObject(data, type, settings);
                return true;
            }
            catch { }
            result = null;
            return false;
        }

        public bool TrySerialize(object @object, out string result)
        {
            try
            {
                result = JsonConvert.SerializeObject(@object, settings);
                return true;
            }
            catch { }
            result = null;
            return false;
        }
    }
}
