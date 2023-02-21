using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Serialization
{
    public class JsonSerializationSettings
    {
        public JsonSerializationTypeNameHandling TypeNameHandling { get; set; }
        public static JsonSerializationSettings Default
        {
            get
            {
                return new JsonSerializationSettings
                {
                    TypeNameHandling = JsonSerializationTypeNameHandling.Auto
                };
            }
        }

        private Newtonsoft.Json.TypeNameHandling ToNewtonsoftTypeNameHamdling(JsonSerializationTypeNameHandling value)
        {
            switch (value)
            {
                case JsonSerializationTypeNameHandling.None:
                    return Newtonsoft.Json.TypeNameHandling.None;
                case JsonSerializationTypeNameHandling.All:
                    return Newtonsoft.Json.TypeNameHandling.All;
                case JsonSerializationTypeNameHandling.Objects:
                    return Newtonsoft.Json.TypeNameHandling.Objects;
                case JsonSerializationTypeNameHandling.Arrays:
                    return Newtonsoft.Json.TypeNameHandling.Arrays;
                default :
                    return Newtonsoft.Json.TypeNameHandling.Auto;



            }
        }
        internal JsonSerializerSettings ToNewtonsoftSerializtionSettings()
        {
            return new Newtonsoft.Json.JsonSerializerSettings
            {
                TypeNameHandling = ToNewtonsoftTypeNameHamdling(this.TypeNameHandling)
            };
        }
        static JsonSerializationSettings()
        {
            JsonConvert.DefaultSettings = () =>
            {
                return JsonSerializationSettings.Default.ToNewtonsoftSerializtionSettings();

            };

        }

    }
}
