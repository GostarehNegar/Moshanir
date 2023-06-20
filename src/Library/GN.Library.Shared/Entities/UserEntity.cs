using GN.Library.Shared.Internals;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace GN.Library.Shared.Entities
{
    public class UserEntity : XrmDynamicEntity
    {
        public new class Schema : DynamicEntity.Schema
        {
            public const string LogicalName = "systemuser";
            public const string SolutionPrefix = "gndync_";
            public const int SolutionOpsionSetBase = 630750000;
            public const string FullName = "fullname";
            public const string DomainName = "domainname";
            public const string ExtensionNumber = SolutionPrefix + "extensionnumber";
            public const string XmppJid = SolutionPrefix + "xmppjid";
            public const string MobilePhone = "mobilephone";
            public const string PreferredNotificationChannel = SolutionPrefix + "preferrednotificationchannel";
            public const string UserName = "username";
            public enum NotificationChannels
            {
                WindowsAgent = SolutionOpsionSetBase,
                XmppAgent = SolutionOpsionSetBase + 1,
                WhatsApp = SolutionOpsionSetBase + 2,
                All = SolutionOpsionSetBase + 3
            }
        }

        public UserEntity()
        {
            this.LogicalName = Schema.LogicalName;
            this.Id = Guid.NewGuid().ToString();
        }
        public string DomainName { get => this.GetAttributeValue(Schema.DomainName); set => this.SetAttributeValue(Schema.DomainName, value); }
        public string MobilePhone { get => this.GetAttributeValue(Schema.MobilePhone); set => this.SetAttributeValue(Schema.MobilePhone, value); }
        public string ExtensionNumber { get => this.GetAttributeValue(Schema.ExtensionNumber); set => this.SetAttributeValue(Schema.ExtensionNumber, value); }
        public override string Name { get => this.FullName; set => this.FullName = value; }
        public string XmppJid { get => this.GetAttributeValue(Schema.XmppJid); set => this.SetAttributeValue(Schema.XmppJid, value); }
        public string FullName { get => this.GetAttributeValue(Schema.FullName) ?? this.GetAttributeValue("name"); set => this.SetAttributeValue(Schema.FullName, value); }
        public Schema.NotificationChannels PreferredNotificationChannel
        {
            get => (Schema.NotificationChannels)this.GetAttributeValue<int>(Schema.PreferredNotificationChannel);
            set => this.SetAttributeValue(Schema.PreferredNotificationChannel, value);
        }


        public DynamicEntity Identity
        {
            get => this.GetAttributeValue<DynamicEntity>("identity");
            set => this.SetAttributeValue("identity", value);
        }
        public string UserName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.GetAttributeValue<string>(Schema.UserName)) && !string.IsNullOrWhiteSpace(this.DomainName))
                {
                    this.SetAttributeValue(Schema.UserName, LibraryConventions.Instance.LoginNameToUserId(this.DomainName));

                }
                return this.GetAttributeValue<string>(Schema.UserName);

            }
            set
            {
                this.SetAttributeValue(Schema.UserName, value);
            }
        }

        public ClaimsIdentity GetClaimsIdentity()
        {
            var result = this.Identity?.To<UserIdentityEntity>()?.GetClaimsIdentity() ?? new GenericIdentity(this.UserName);
            result.AddClaim(new Claim(ClaimTypesEx.CrmUserId, this.Id));
            return result;
        }
    }
}
