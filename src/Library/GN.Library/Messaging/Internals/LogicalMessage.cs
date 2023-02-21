using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Internals
{

    /// <summary>
    /// Logical container for a message that helps in packaging the message
    /// for transportation.
    /// </summary>
    public class LogicalMessage : ILogicalMessage
    {
        protected Type _messageType;
        protected string _messageTypeName;
        protected object _message;
        protected string _data;
        protected MessageHeader header;
        protected string _id;
        private IMessagingSerializationService serializer;
        public MessageTopic Topic { get; protected set; }
        public string Subject => this.Topic?.Subject;
        public string Stream => this.Topic?.Stream;
        public long? Version => this.Topic?.Version;
        public ILogicalMessage WithTopic(string subject, string stream, long? version)
        {
            this.Topic = MessageTopic.Create(subject, stream, version);
            return this;
        }

        /// <summary>
        /// Constructs a logical message as a warpper of the a message object that
        /// is suitable for packaging for transportation.
        /// <para/>
        /// Note that one may construct a logical message with a valid object or
        /// with a serialized string and a valid message type. I latter case
        /// the wrapped object will be deserialized later in GetBody method.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        /// <param name="properties"></param>
        /// <param name="messageType"></param>
        public LogicalMessage(MessageTopic topic, object message, IDictionary<string, string> properties = null, Type messageType = null, string typeName = null, string data = null)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            this.serializer = SerializationService.Default;
            this.Topic = topic ?? MessageTopic.Create("$no-topic");
            this._message = message;
            this._messageType = typeName == null
                ? messageType ?? message?.GetType()
                : MessageTopicHelper.GetTypeByName(typeName);

            this._messageTypeName = typeName ?? messageType?.AssemblyQualifiedName ?? message?.GetType().AssemblyQualifiedName;
            this.header = new MessageHeader(properties ?? new Dictionary<string, string>());
            this._id = Guid.NewGuid().ToString();
            this.Timestamp(DateTime.UtcNow);
            bool isjon(object o)
            {
                return o != null && o is string p && p != null
                    && p.Length > 0 && (p[0] == '"' || p[0] == '{' || p[0] == '[');
            }
            if (message == null)
            {
                this._data = null;
            }
            else if (message != null && message is MessagePack pack)
            {
                //this._messageTypeName = pack.Subject;
                this._messageTypeName = pack.TypeName ?? pack.Subject;
                this._messageType = typeof(object);
                this._data = pack.Payload;
                this._message = null;
            }
            else if (this._message is JObject jo)
            {
                this._data = jo.ToString();
                this._message = null;
            }
            else if (isjon(this._message))
            {
                this._data = (string)this._message;
                this._message = null;
            }
            else
            {
                this._data = this.serializer.Serialize(this._message);
                if (this._message.GetType() == typeof(string))
                {
                    this._message = null;
                }
            }
        }
        private bool TryGetType(out Type type)
        {
            return MessageTopicHelper.TryGetTypeByName(this._messageTypeName, out type);
        }
        protected object GetBody(Type type = null, bool convert = false)
        {
            if (this.TryGetBody(out var res, type, convert))
            {
                this._message = res;
                //if (convert && res != null)
                //{
                //    this._messageType = res.GetType();
                //    this._messageTypeName = res.GetType().AssemblyQualifiedName;
                //}
                return res;
            }
            return null;
        }
        protected bool TryGetBody(out object result, Type type = null, bool convert = false)
        {
            var target_type = (type == null || type == typeof(object)) ? this._messageType : type;
            target_type = target_type ?? this.GetMessageType() ?? typeof(object);
            bool ret = false;
            result = this._message;
            ret = result != null && target_type != typeof(object) && target_type.IsAssignableFrom(result.GetType());
            if (!ret && result != null && result is JObject jobject)
            {
                ret = this.serializer.TryToObject(jobject, target_type, out result);
            }
            if (!ret && this._data != null)
            {
                ret = this.serializer.TryDeserialize(this._data, target_type, out result);
            }
            if (ret && convert && result != null)
            {
                this._messageType = result.GetType();
                this._messageTypeName = result.GetType().AssemblyQualifiedName;
            }
            ret = ret && result != null && target_type.IsAssignableFrom(result.GetType());
            result = ret ? result : null;
            return ret;
        }
        public T GetBody<T>(bool convert = false)
        {
            var result = this.GetBody(typeof(T), convert);
            if (result != null && typeof(T).IsAssignableFrom(result.GetType()))
            {
                return (T)result;
            }
            return default(T);
        }
        public object Body => GetBody();
        public IMessageHeader Headers => this.header;
        public string MessageId => this._id;
        public LogicalMessage<T> Cast<T>()
        {
            return TryCast<T>(out var _result)
                ? _result
                : null;
        }
        /// <summary>
        /// Tries to cast the body to the desired type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryCast<T>(out LogicalMessage<T> result)
        {
            if (this.TryGetBody(out var tmp, typeof(T), true))
            {
                result = new LogicalMessage<T>(this.Topic, (T)tmp, this.header);
                result._id = this._id;
                return true;
            }
            result = null;
            return false;
            //this.GetBody();
            //if (this._message != null && typeof(StreamPack).IsAssignableFrom(this._message.GetType()) && typeof(T) != typeof(StreamPack))
            //{
            //    var f = this._message as StreamPack;
            //    if (f != null)
            //    {
            //        try
            //        {
            //            var m = this.serializer.Deserialize<T>(f.Payload);
            //            result = new LogicalMessage<T>(this.Topic, m, this.header);
            //            result._id = this._id;
            //            return true;
            //        }
            //        catch (Exception)
            //        {

            //        }
            //    }
            //}
            //if (this._message != null && this._message is JObject o)
            //{
            //    result = new LogicalMessage<T>(this.Topic, o.ToObject<T>(), this.header);
            //    result._id = this._id;
            //    return true;
            //}
            //if (this._message != null && typeof(T).IsAssignableFrom(this._message.GetType()))
            //{
            //    result = new LogicalMessage<T>(this.Topic, (T)this._message, this.header);
            //    result._id = this._id;
            //    return true;
            //}
            //result = null;
            //return false;
        }
        public Type GetMessageType()
        {
            //if (!string.IsNullOrWhiteSpace(this._messageTypeName) && MessageTopicHelper.GetTypeByName(this._messageTypeName) == null)
            //{
            //    return typeof(object);
            //}
            return this._messageType ?? MessageTopicHelper.GetTypeByName(this._messageTypeName) ?? typeof(object);
        }
        public bool TryGetHeaderValue<T>(string key, out T result)
        {
            return this.Headers.TryGetValue<T>(key, out result);
        }
        public void SetTopic(MessageTopic routing)
        {
            this.Topic = routing ?? MessageTopic.Create("$notopic");
        }
        /// <summary>
        /// Packs the message in a 'MessagePack' object that can be wired.
        /// </summary>
        /// <returns></returns>
        public MessagePack Pack(bool camel = false)
        {
            var result = new MessagePack();
            camel = camel || this.Headers.JsonFormat() == "camel";
            result.Payload = (this._message == null ? null : this.serializer.Serialize(this._message, camel))
                            ?? this._data;
            result.TypeName = this._messageTypeName;
            result.Headers = new Dictionary<string, string>(this.Headers);
            result.SetTopic(this.Topic);
            result.Id = this.MessageId;
            return result;
        }

        public static LogicalMessage<object> Unpack(MessagePack pack)
        {
            var result = new LogicalMessage<object>(pack.GetTopic(), pack.Payload, pack.Headers, pack.GetPayloadType(), pack.TypeName);
            result._id = pack.Id;
            return result;
        }

        public DateTime? Timestamp(DateTime? stamp)
        {
            if (stamp.HasValue)
            {
                this.Headers.TrySetValue<DateTime>("$timestamp", stamp.Value);
            }
            return this.Headers.GetValue<DateTime?>("$timestamp");
        }
        public string From(string endpoint = null)
        {
            if (endpoint != null)
            {
                this.Headers.TrySetValue(MessagingConstants.HeaderKeys.From, endpoint);
            }
            return this.Headers.GetValue<string>(MessagingConstants.HeaderKeys.From);
        }
        public string To(string endpoint = null)
        {
            if (endpoint != null)
            {
                this.Headers.TrySetValue(MessagingConstants.HeaderKeys.To, endpoint);
            }
            return this.Headers.GetValue<string>(MessagingConstants.HeaderKeys.To);
        }
        public string InReplyTo(string value = null)
        {
            if (value != null)
            {
                this.Headers.TrySetValue(MessagingConstants.HeaderKeys.InReplyTo, value);
            }
            return this.Headers.GetValue<string>(MessagingConstants.HeaderKeys.InReplyTo);
        }

        public LogicalMessage WithVersion(long? version)
        {
            this.Topic?.SetVersion(version);
            return this;
        }

        ILogicalMessage<T> ILogicalMessage.Cast<T>()
        {
            return this.Cast<T>();
        }

        bool ILogicalMessage.TryCast<T>(out ILogicalMessage<T> result)
        {
            if (this.TryCast<T>(out var _result))
            {
                result = _result;
                return true;
            }
            result = null;
            return false;
        }
    }
    public class LogicalMessage<T> : LogicalMessage, ILogicalMessage<T>
    {
        public LogicalMessage(MessageTopic topic, T message, IDictionary<string, string> properties, Type type = null, string typeName = null) : base(topic, message, properties, type, typeName)
        {
            if (topic == null && typeof(T) != typeof(object) && !typeof(T).IsPrimitive)
                this.SetTopic(MessageTopic.Create(typeof(T)));
        }
        public new T Body => (T)this.GetBody();
    }
}
