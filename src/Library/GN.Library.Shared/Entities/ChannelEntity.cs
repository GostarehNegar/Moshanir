using GN.Library.Shared.Chats;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GN.Library.Shared.Entities
{
    public static class ChannelTypes
    {
        public const string Direct = "direct";
        public const string Chat = "chat";
        public const string GroupChat = "groupchat";
        public const string EntityChat = "entitychat";
    }

    public class ChannelEntity : DynamicEntity
    {
        public new class Schema : DynamicEntity.Schema
        {
            public const string LogicalName = "channel";
            public const string ChannelTyep = "channeltype";
            public const string ChatUsers = "chatusers";

        }
        public ChannelEntity() : base()
        {
            LogicalName = Schema.LogicalName;
        }
        public virtual string Description { get => GetAttributeValue(Schema.Description); set => SetAttributeValue(Schema.Description, value); }
        public ChatUserEntity[] ChatUsers { get => this.GetAttributeValue<ChatUserEntity[]>(Schema.ChatUsers); set => this.SetAttributeValue(Schema.ChatUsers, value); }
        public string ChannelType { get => this.GetAttributeValue(Schema.ChannelTyep); set => this.SetAttributeValue(Schema.ChannelTyep, value); }
        public long Version { get; set; }
        public ChatUserEntity GetPrimaryUserEntity()
        {
            if (ChannelType == ChannelTypes.Direct && this.ChatUsers != null && this.ChatUsers.Length > 0)
            {
                return this.ChatUsers[0];
            }
            return null;
        }
        public void AddUserEntity(ChatUserEntity user)
        {
            var users = new List<ChatUserEntity>(this.ChatUsers ?? Array.Empty<ChatUserEntity>());
            users.Add(user);
            this.ChatUsers = users.ToArray();
        }
        public string GetDisplayName(string currentUser)
        {
            return this.ChatUsers.FirstOrDefault(x => x.Id != currentUser)?.Name;
        }

    }
}
