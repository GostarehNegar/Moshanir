using GN.Library.Messaging.Internals;
using System;

namespace GN.Library.Messaging
{
    public interface ILogicalMessage
    {
        string MessageId { get; }
        string Subject { get; }
        string Stream { get; }
        long? Version { get; }
        IMessageHeader Headers { get; }
        MessagePack Pack(bool camel = false);
        object Body { get; }
        Type GetMessageType();
        void SetTopic(MessageTopic subject);
        T GetBody<T>(bool convert = false);
        ILogicalMessage<T> Cast<T>();
        bool TryCast<T>(out ILogicalMessage<T> result);
        //bool TryConvert<T>(out LogicalMessage<T> result);
        ILogicalMessage WithTopic(string subject, string stream, long? version);

    }
    public interface ILogicalMessage<T> : ILogicalMessage
    {
        new T Body { get; }
    }
}
