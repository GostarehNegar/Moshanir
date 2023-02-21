using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Chats
{
    public class ChannelSubscriptionEntity : DynamicEntity
    {
        public new class Schema
        {
            public const string LogicalName = "subscription";
            public const string SubscriberId = "subscriberid";
            public const string ChannelId = "channelid";
        }
        public ChannelSubscriptionEntity()
        {
            this.Id = Guid.NewGuid().ToString();
            this.LogicalName = Schema.LogicalName;
        }
        public string SubscriberId { get => this.GetAttributeValue(Schema.SubscriberId); set => this.SetAttributeValue(Schema.SubscriberId, value); }
        public string ChannelId { get => this.GetAttributeValue(Schema.ChannelId); set => this.SetAttributeValue(Schema.ChannelId, value); }

    }
}
