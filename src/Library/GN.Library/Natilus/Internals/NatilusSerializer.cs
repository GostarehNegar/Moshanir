using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Natilus.Internals
{
    class NatilusSerializer : INatilusSerializer
    {
        public static NatilusSerializer Default = new NatilusSerializer();

        public T Deserialize<T>(byte[] data)
        {
            if (data == null || data.Length == 0)
                return default(T);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(data));
        }

        public byte[] Serialize(object obj)
        {
            return System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
        }
    }
}
