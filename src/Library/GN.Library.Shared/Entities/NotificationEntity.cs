using GN.Library.Shared.Chats;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Entities
{
    public class NotificationEntity : DynamicEntity
    {
        public new class Schema : DynamicEntity.Schema
        {
            public const string SolutionPrefix = "gndync_";
            public const string LogicalName = SolutionPrefix + "notification";
            public const string Subject = "subject";
            public const string OwnerId = "ownerid";
            public const string EntityURL = SolutionPrefix + "relatedentityurl";


        }
        public string Subject
        {
            get => GetAttributeValue(Schema.Subject);
            set => SetAttributeValue(Schema.Subject, value);
        }
        public DynamicEntityReference Owner
        {
            get => GetAttributeValue<DynamicEntityReference>(Schema.OwnerId);

        }
        public string EntityURL
        {
            get => GetAttributeValue(Schema.EntityURL);
            set => SetAttributeValue(Schema.EntityURL, value);
        }
        public string GetNotificationBody()
        {
            return Subject + " .info: " + EntityURL;
        }

    }
}
