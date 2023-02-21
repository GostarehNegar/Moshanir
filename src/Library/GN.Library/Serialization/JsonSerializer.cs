using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Serialization
{
    public class JsonSerializerEx : IJsonSerializer
    {

        public void test()
        {

        }

        public string Serialize(object value, JsonSerializationSettings settings = null)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, settings == null
                ? JsonSerializationSettings.Default.ToNewtonsoftSerializtionSettings()
                : settings.ToNewtonsoftSerializtionSettings());
        }
        public string Serialize2(object value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, new Newtonsoft.Json.JsonSerializerSettings 
            { 
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None
            });
        }


        public T Deserialize<T>(string value, JsonSerializationSettings settings = null)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value, settings == null
                ? JsonSerializationSettings.Default.ToNewtonsoftSerializtionSettings()
                : settings.ToNewtonsoftSerializtionSettings());
        }


        public object Deserialize(Type type, string value, JsonSerializationSettings settings = null)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject( value, type, settings == null
                ? JsonSerializationSettings.Default.ToNewtonsoftSerializtionSettings()
                : settings.ToNewtonsoftSerializtionSettings());
        }
    }
}
