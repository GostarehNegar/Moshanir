using GN.Library.Natilus.Internals;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GN.Library.Messaging.Internals;
using GN.Library.Messaging;
using GN.Library.Shared.Internals;

namespace GN.Library.Natilus.Messaging
{
    public class NatilusMessageHeader : Dictionary<string, string>
    {
        public NatilusMessageHeader(IDictionary<string, string> dictionary = null) : base(dictionary ?? new Dictionary<string, string>())
        {

        }

    }
    public class NatilusMessage
    {
        private readonly string subject;
        private byte[] data;
        private readonly INatilusSerializer serializer;
        public NatilusMessageHeader Headers = new NatilusMessageHeader();
        public string Subject => this.subject;
        private Msg Msg;
        public string Id { get => this.GetHeaderValue<string>("$id"); set => this.SetHeaderValue("$id", value); }
        public NatilusMessage(string subject, byte[] data, IDictionary<string, string> headers, INatilusSerializer serializer = null)
        {
            this.subject = subject;
            this.data = data ?? new byte[] { };
            this.serializer = serializer ?? NatilusSerializer.Default;
            this.Headers = new NatilusMessageHeader(headers ?? new NatilusMessageHeader());
            this.Msg = new Msg(this.subject, this.GetMsgHeader(), this.Data);
            
        }
        internal NatilusMessage(Msg msg, INatilusSerializer serializer = null)
        {
            this.Msg = msg;
            this.subject = msg.Subject;
            this.serializer = serializer ?? NatilusSerializer.Default;
            this.data = msg.Data;
            this.Headers = new NatilusMessageHeader();
            foreach (var h in msg.Header.Keys.OfType<string>())
            {
                this.Headers[h] = msg.Header[h];
            }
        }


        public NatilusMessage(string subject, object data, IDictionary<string, string> headers, INatilusSerializer serializer = null)
        {
            this.Headers = new NatilusMessageHeader(headers ?? new NatilusMessageHeader());
            this.Id = Guid.NewGuid().ToString();
            this.subject = subject;
            this.serializer = serializer ?? NatilusSerializer.Default;
            this.data = this.serializer.Serialize(data);
            if (data != null)
            {
                this.TypeName(data.GetType().AssemblyQualifiedName);
            }
            this.Msg = new Msg(this.Subject, this.GetMsgHeader(), this.Data);
        }
        public string From(string endpoint=null)
        {
            if (endpoint != null)
            {
                this.SetHeaderValue("$from", endpoint);
            }
            return this.GetHeaderValue<string>("$from");
        }

        private MsgHeader GetMsgHeader()
        {
            var h = new MsgHeader();
            foreach (var p in this.Headers)
            {
                h.Add(p.Key, p.Value);
            }
            return h;
        }
        public void SetHeaderValue(string header, object value)
        {
            this.Headers[header] = value?.ToString();
        }
        public T GetHeaderValue<T>(string key)
        {
            return this.Headers.TryGetValue(key, out var res) ? NatilusExtensions.ParsePrimitive<T>(res) : default(T);
        }
        //public LogicalMessage<T> GetLogicalMessage<T>()
        //{
        //    return new LogicalMessage<T>(MessageTopic.Create(this.Subject), GetData<T>(), this.Headers);
        //    //var pack = new MessagePack()
        //    //{
        //    //    TypeName = this.TypeName(),
        //    //    Id = this.Id,
        //    //    Headers = new Dictionary<string, string>(this.Headers),
        //    //    Payload = System.Text.Encoding.UTF8.GetString(this.Data)
        //    //};
        //    //pack.SetTopic(MessageTopic.Create(this.Subject));
        //    //return LogicalMessage<T>.Unpack(pack);
        //}
        public LogicalMessage<object> GetLogicalMessage()
        {
            //return new LogicalMessage<object>(MessageTopic.Create(this.Subject), GetData<T>(), this.Headers);
            var pack = new MessagePack()
            {
                TypeName = this.TypeName(),
                Id = this.Id,
                Headers = new Dictionary<string, string>(this.Headers),
                Payload = System.Text.Encoding.UTF8.GetString(this.Data)
            };
            pack.SetTopic(MessageTopic.Create(this.Subject));
            return LogicalMessage<object>.Unpack(pack);
        }
        public string TypeName(string val = null)
        {
            if (val != null)
            {
                this.SetHeaderValue("$type_name", val);
            }
            return this.GetHeaderValue<string>("$type_name");
        }

        public byte[] Data
        {
            get
            {
                this.data = this.data ?? new byte[] { };
                return this.data;

            }
            set
            {
                this.data = value == null ? new byte[] { } : value;
            }
        }
        public T GetData<T>()
        {
            return this.serializer.Deserialize<T>(this.Data);
        }

        internal NATS.Client.Msg ToMsg()
        {
            return new NATS.Client.Msg(this.subject, this.GetMsgHeader(), this.Data);
        }
        public LogicalMessage<T> ToLogicalMessage<T>()
        {
            return new LogicalMessage<T>(MessageTopic.Create(this.Subject), GetData<T>(), this.Headers);
        }


    }
}
