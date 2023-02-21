using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GN.Library.Messaging.Internals
{
    public interface IMessagingSerializationService
    {
        MessagePack ToVersionableEvent(object @object);
        bool TrySerialize(object @object, out string result, bool camel = false);
        bool TryEncode(object @object, out byte[] result);
        bool TryDecode(byte[] data, Type type, out object result);
        bool TryDeserialize(string data, Type type, out object result);
        bool TryDeserialize<T>(string data, out T result);
        T Deserialize<T>(string data);
        string Serialize(object @object, bool camel = false);
        bool TryDecode<T>(byte[] data, out T result);
        T Decode<T>(byte[] data);
        string ByteArrayToString(byte[] data);
        byte[] StringToByteArray(string data);
        bool TryToObject(JObject jobject, Type type, out object result);

    }

    class SerializationService : IMessagingSerializationService
    {
        public static IMessagingSerializationService Default = AppHost.Initailized
            ? (AppHost.GetService<IMessagingSerializationService>() ?? new SerializationService())
            : new SerializationService();
        static JsonSerializerSettings settings = new JsonSerializerSettings();
        static JsonSerializerSettings CAMEL_SETTINGS =
            new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

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
        public bool TrySerialize(object @object, out string result, bool camel = false)
        {
            try
            {
                result = camel
                    ? JsonConvert.SerializeObject(@object, CAMEL_SETTINGS)
                    : JsonConvert.SerializeObject(@object, settings);
                return true;
            }
            catch { }
            result = null;
            return false;
        }
        public SerializationService()
        {

        }
        public string SerializeMessagePack(MessagePack packet)
        {
            return Serialize(packet);
            //return Newtonsoft.Json.JsonConvert.SerializeObject(packet);
            //return Encoding.UTF8.GetBytes(str);
        }
        public Byte[] EncodeMessagePack(MessagePack packet)
        {
            //var str = Newtonsoft.Json.JsonConvert.SerializeObject(packet);
            return StringToByteArray(SerializeMessagePack(packet));
        }
        public MessagePack DecodeMessagePack(byte[] bytes)
        {
            return Deserialize<MessagePack>(ByteArrayToString(bytes));
            //return Newtonsoft.Json.JsonConvert.DeserializeObject<MessagePack>(
            //    Encoding.UTF8.GetString(bytes));
        }
        public MessagePack DeserializMessagePack(string str)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<MessagePack>(str);

        }
        public MessagePack ToVersionableEvent(object @object)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<MessagePack>(
                    Newtonsoft.Json.JsonConvert.SerializeObject(@object));
            }
            catch (Exception err)
            {

            }
            return null;
        }

        public bool TryEncode(object @object, out byte[] result)
        {
            if (TrySerialize(@object, out var _res))
            {
                result = StringToByteArray(_res);
                return true;
            }
            result = null;
            return false;
        }

        public bool TryDecode(byte[] data, Type type, out object result)
        {
            if (data == null || data.Length == 0)
            {
                result = null;
                return true;
            }
            if (TryDeserialize(ByteArrayToString(data), type, out var _res))
            {
                result = _res;
                return true;
            }
            result = null;
            return false;
        }

        public bool TryDeserialize<T>(string data, out T result)
        {
            if (TryDeserialize(data, typeof(T), out var _result))
            {
                result = (T)_result;
                return true;
            }
            result = default;
            return false;
        }

        public T Deserialize<T>(string data)
        {
            return TryDeserialize<T>(data, out var result) ? result : default;
        }

        public string Serialize(object @object, bool camel = false)
        {
            return TrySerialize(@object, out var result, camel) ? result : null;
        }

        public bool TryDecode<T>(byte[] data, out T result)
        {
            if (TryDecode(data, typeof(T), out var _result))
            {
                result = (T)_result;
                return true;
            }
            result = default;
            return false;
        }
        public T Decode<T>(byte[] data)
        {
            return TryDecode<T>(data, out var result) ? result : default;
        }

        public string ByteArrayToString(byte[] data)
        {
            return System.Text.Encoding.UTF8.GetString(data);
        }

        public byte[] StringToByteArray(string data)
        {
            return System.Text.Encoding.UTF8.GetBytes(data);
        }

        public bool TryToObject(JObject jobject, Type type, out object result)
        {
            result = null;
            try
            {
                result = jobject.ToObject(type);
                return true;
            }
            catch { }
            return false;
        }
    }
}
