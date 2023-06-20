using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Linq;
using System.Security.Principal;
using GN.Library.Shared;

namespace GN.Library.Messaging
{
    public static partial class MessagingExtensions
    {

        public static string QueueEndpoint(this IMessageHeader header, string value = null)
        {
            if (value != null)
            {
                header["#queue-endpoint"] = value;
            }
            return header.GetValue<string>("#queue-endpoint");
        }
        public static IMessageContext To(this IMessageContext context, string value)
        {
            context.Message.Headers.To(value);
            return context;
        }
        public static void SetTopic(this MessagePack This, MessageTopic topic)
        {
            This.Subject = topic?.Subject;
            This.Version = topic?.Version ?? -1;
            This.Stream = topic?.Stream;
            return;
        }
        public static MessageTopic GetTopic(this MessagePack This)
        {
            return MessageTopic.Create(This.Subject, This.Stream, This.Version);
        }
        public static Type GetPayloadType(this MessagePack This)
        {
            try
            {
                return MessageTopicHelper.GetTypeByName(This.TypeName);
            }
            catch { }
            return null;
        }
        public static bool IsVersiableEvent(this IMessageHeader This, bool? value = null)
        {
            if (value.HasValue)
            {
                This.TrySetValue<bool>("$is-versionable-event", value.Value);
            }
            return This.GetValue<bool>("$is-versionable-event");
        }
        public static DateTime? Timestamp(this IMessageHeader header, DateTime? stamp)
        {
            if (stamp.HasValue)
            {
                header.TrySetValue<DateTime>("$timestamp", stamp.Value);
            }
            return header.GetValue<DateTime?>("$timestamp");
        }
        public static DateTime? Timestamp(this ILogicalMessage message, DateTime? stamp)
        {
            return message.Headers.Timestamp(stamp);
        }
        public static ILogicalMessage WithVersion(this ILogicalMessage This, long? version)
        {
            This.WithTopic(This.Subject, This.Stream, This.Version);
            //This.Topic?.SetVersion(version);
            return This;
        }
        public static string To(this IMessageHeader This, string endpoint = null)
        {
            if (endpoint != null)
            {
                This.TrySetValue(MessagingConstants.HeaderKeys.To, endpoint);
            }
            return This.GetValue<string>(MessagingConstants.HeaderKeys.To);
        }
        public static string To(this ILogicalMessage This, string endpoint = null)
        {
            return This.Headers.To(endpoint);
        }
        public static string ReplayFor(this ILogicalMessage message, string value = null)
        {
            if (message == null)
                return null;
            return message.Headers.ReplayFor(value);
        }
        public static string ReplayFor(this LogicalMessage message, string value = null)
        {
            if (message == null)
                return null;
            return message.Headers.ReplayFor(value);
        }
        //public static bool SkipSaveToStream(this LogicalMessage message, bool? value = null)
        //{
        //    if (value.HasValue)
        //    {
        //        message.Headers.TrySetValue("$skip-save-to-stream", true);
        //    }
        //    return message.Headers.GetValue<bool>("$skip-save-to-stream");

        //}
        public static string ReplayFor(this IMessageHeader header, string value = null)
        {
            if (header == null)
                return null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                header.TrySetValue("$replay-for", value);
            }
            return header.GetValue<string>("$replay-for");
        }
        public static string ReplayTo(this IMessageHeader header, string value = null)
        {
            if (header == null)
                return null;
            if (!string.IsNullOrWhiteSpace(value))
            {
                header.TrySetValue("$replay-to", value);
            }
            return header.GetValue<string>("$replay-to");
        }
        public static bool IsReplayingForMe(this IMessageHeader header, string myEndpoint)
        {
            return header.ReplayTo() == myEndpoint;

        }
        public static string InReplyTo(this IMessageHeader This, string value = null)
        {
            if (value != null)
            {
                This.TrySetValue(MessagingConstants.HeaderKeys.InReplyTo, value);
            }
            return This.GetValue<string>(MessagingConstants.HeaderKeys.InReplyTo);
        }
        public static string InReplyTo(this ILogicalMessage This, string value = null)
        {
            return This?.Headers.InReplyTo(value);
        }
        public static string StreamEndpoint(this IMessageHeader This, string endpoint = null)
        {
            if (endpoint != null)
            {
                This.TrySetValue("$stream-endpoint", endpoint);
            }
            return This.GetValue<string>("$stream-endpoint");
        }
        public static bool IsStreamReplay(this IMessageHeader This)
        {
            return !string.IsNullOrWhiteSpace(This.StreamEndpoint());
        }
        public static string From(this IMessageHeader This, string endpoint = null)
        {
            if (endpoint != null)
            {
                This.TrySetValue(MessagingConstants.HeaderKeys.From, endpoint);
            }
            return This.GetValue<string>(MessagingConstants.HeaderKeys.From);
        }
        public static string From(this ILogicalMessage This, string endpoint = null)
        {
            return This.Headers.From(endpoint);
        }
        public static byte[] ToByteArray(this ILogicalMessage message)
        {
            return message == null
                ? new byte[] { }
                : Services.GetSerializationService().EncodeMessagePack(message.Pack());
        }
        public static string Serialize(this ILogicalMessage message)
        {
            return message == null
                ? string.Empty
                : Services.GetSerializationService().SerializeMessagePack(message.Pack());
        }

        public static byte[] ToByteArray(this MessagePack pack)
        {
            return Services.GetSerializationService().EncodeMessagePack(pack);
        }

        public static MessagePack ToMessagePack(this byte[] pack)
        {
            return Services.GetSerializationService().DecodeMessagePack(pack);
        }

        public static MessageFlags Flags(this IMessageHeader header, MessageFlags? value = null)
        {
            if (value.HasValue)
            {
                header["$flags"] = ((int)value).ToString();
            }
            return (MessageFlags)header.GetValue<int>("$flags");
        }
        public static bool HasFlag(this IMessageHeader header, MessageFlags flag)
        {
            return (header.Flags() & flag) == flag;
        }

        public static string ErrorMessage(this IMessageHeader header,string value)
        {
            if (value !=null)
            {
                header[LibraryConstants.MessagingConstants.HeaderKeys.ErrorMessage] = value;
                //header.TrySetValue<int>("statuscode", value.Value);
            }
            return header.GetValue<string>(LibraryConstants.MessagingConstants.HeaderKeys.ErrorMessage);


        }
        public static int StatusCode(this IMessageHeader header, int? value)
        {
            if (value.HasValue)
            {
                //header["statuscode"] = value.Value.ToString();
                header.TrySetValue<int>(LibraryConstants.MessagingConstants.HeaderKeys.StatusCode, value.Value);
            }
            return header.GetValue<int>(LibraryConstants.MessagingConstants.HeaderKeys.StatusCode);


        }
        public static IMessageHeader AddFlag(this IMessageHeader header, MessageFlags flag)
        {
            header.Flags(header.Flags() | flag);
            return header;
        }
        public static IMessageHeader RemoveFlag(this IMessageHeader header, MessageFlags flag)
        {
            header.Flags(header.Flags() & ~flag);
            return header;
        }

        public static bool IsQueuedMessage(this ILogicalMessage message)
        {
            return message.Headers.HasFlag(MessageFlags.QueuedMessage);
        }

        public static IMessageContext WithMessage(this IMessageContext context, Action<ILogicalMessage> action)
        {
            action?.Invoke(context.Message);
            return context;
        }


        public static string JsonFormat(this IMessageHeader header, string value = null)
        {
            if (value != null)
            {
                header.TrySetValue("jsonformat", value);
            }
            return header.GetValue<string>("jsonformat");
        }

        public static ClaimsIdentity Identity(this IMessageHeader header, ClaimsIdentity value = null)
        {
            if (value != null)
            {
                var dic = value.Claims
                    .ToDictionary(x => x.Type, x => x.Value);
                header.TrySetValue("#identity", SerializationService.Default.Serialize(dic));
            }
            if (header.TryGetValue("#identity", out var _res))
            {
                var dic = string.IsNullOrWhiteSpace(_res) ? null :
                    SerializationService.Default.Deserialize<Dictionary<string, string>>(_res);
                if (dic != null)
                {
                    var res = new GenericIdentity(dic[ClaimTypes.Name]);
                    dic
                        .Where(x => x.Key != ClaimTypes.Name)
                        .ToList()
                        .ForEach(x => res.AddClaim(new Claim(x.Key, x.Value)));
                    return res;

                }
            }
            return null;
        }
        public static Guid? CrmUserId(this IMessageHeader header)
        {
            return header.Identity().GetCrmUserId();
        }

        //public static string Queue(this IMessageHeader header, string value = null)
        //{
        //    if (value != null)
        //    {
        //        header["#queue"] = value;
        //    }
        //    return header.GetValue<string>("#queue");
        //}

        public static IMessageContext QueueMessage(this IMessageContext context, IMessageContext value = null)
        {
            return value == null
                ? context.GetProperty<IMessageContext>("$queuemesssage", null)
                : context.GetProperty<IMessageContext>("$queuemesssage", () => value);
        }


    }
}
