using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Serialization
{
    public interface IJsonSerializer
    {

        string Serialize(object value, JsonSerializationSettings settings = null);
        string Serialize2(object value);
        //object Deserialize(string value, Type type, JsonSerializationSettings settings = JsonSerializationSettings.Default);
        T Deserialize<T>(string value, JsonSerializationSettings settings =null);
        object Deserialize(Type type, string value, JsonSerializationSettings settings = null);

    }
}
