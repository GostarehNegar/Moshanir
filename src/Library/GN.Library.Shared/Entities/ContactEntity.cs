using GN.Library.Shared.Chats;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Entities
{
    public class ContactEntity : XrmDynamicEntity
    {
        public new class Schema : DynamicEntity.Schema
        {
            public const string LogicalName = "contact";
            public const string FirstName = "firstname";
            public const string LastName = "lastname";
            public const string Account = "parentcustomerid";
            public const string FullName = "fullname";
            public const string MobilePhone = "mobilephone";
        }
        public ContactEntity()
        {
            LogicalName = Schema.LogicalName;
            Id = Guid.NewGuid().ToString();
        }
        public ChatAccountEntity Account { get => GetAttributeValue<ChatAccountEntity>(Schema.Account); }
        public override string Name { get => FullName; set => FullName = value; }
        public string FullName { get => GetAttributeValue(Schema.FullName) ?? GetAttributeValue("name"); set => SetAttributeValue(Schema.FullName, value); }
        public string FirstName { get => GetAttributeValue(Schema.FirstName); set => SetAttributeValue(Schema.FirstName, value); }
        public string LastName { get => GetAttributeValue(Schema.LastName); set => SetAttributeValue(Schema.LastName, value); }
        public string MobilePhone { get => GetAttributeValue(Schema.MobilePhone); set => SetAttributeValue(Schema.MobilePhone, value); }
    }
}
