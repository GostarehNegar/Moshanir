using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Serialization
{
    /// <summary>
    /// A json converter that can be used to serialize objects with 'Typed Name Handling'.
    /// Its main purpose is to set the 'TypeNameHandling' serialization settings to 'Auto'.
    /// Typically it will be used as the argument for 'JsonConverter' attribute on the target
    /// class.
    /// </summary>
    /// <remarks>
    /// The 'TypeNameHandling' ctx of Newtonsoft json serializer may be used to store
    /// object type information that will be used later on in deserialization process. This
    /// feature should be used as an option either passed while serializaing a specific instance
    /// or be set as a global setting. 
    /// Our converter may be used to declaratively specify that option on a specific class so that
    /// any serialization/deserialization with be done with 'TypeNameHandling'.
    /// The idea is very simple, we will create a custom converter that turns on that 
    /// settings before each serialization/deserialization process.
    /// This implementation is based on overriding 'CustomCreationConverter'. I assume that there 
    /// sure is a better way, but because of lack of detail information about creating a custom
    /// converter, this simple approach has been accepted.
    /// The trick here is to override 'CanWrite'. On the first call we return 'True' so that
    /// our 'WriteJson' method is called. The 'WriteJson' just sets the 'TypeNameHandling' option
    /// to 'Auto' and asks the serializer to proceed, but just before that sets a 'flag' so that
    /// this time 'CanWrite' returns flase and we are not recuresed, otherwise a recursive call exception will 
    /// be thrown by the 'serializer'.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class TypedJsonConverter<T> : Newtonsoft.Json.Converters.CustomCreationConverter<T>
        where T : class,new()
    {
        private bool _flag = false;

        public TypedJsonConverter()
        {

        }

        public override T Create(Type objectType)
        {
            return new T();
        }
        public override bool CanConvert(Type objectType)
        {
            return base.CanConvert(objectType);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
			
            serializer.TypeNameHandling = TypeNameHandling.Auto;
            var result =  base.ReadJson(reader, objectType, existingValue, serializer);

            return result;
        }
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            /// Set the 'TypeNameHandling' ctx to 'Auto'
            serializer.TypeNameHandling = TypeNameHandling.Auto;

            /// Set the flag to avoid recursive calls.
            /// Note that when 'CanWrite' returns false we 
            /// will no longer be called.
            _flag = true;

            /// Proceed with normal serialization but with 
            /// 'Auto TypeNameHandling'
            /// 
            
            serializer.Serialize(writer, value);

            /// Reset the flag for future possible
            /// calls.
            _flag = false;
        }
        public override bool CanWrite
        {
            get
            {
                /// We should only return true for the first call, this
                /// will cause the serializer to call our 'WriteJson' method that
                /// will swicth the flag, so that in future calls we will return
                /// false and avoid the recursive calls.
                return !_flag;
            }
        }
    }

}
