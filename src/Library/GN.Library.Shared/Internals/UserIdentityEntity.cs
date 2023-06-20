
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Linq;
using GN.Library.Shared;
using GN.Library.Shared.Entities;

namespace GN.Library.Shared.Internals
{
    public class UserIdentityEntity : DynamicEntity
    {
        public new class Schema
        {
            public const string GroupNames = "groups";
            public const string Title = "title";
            public const string UserName = "username";
            public const string DisplayName = "displayname";
            public const string Email = "email";
            public const string IpPhoneExtension = "ipphoneextension";
            public const string IsDisabled = "isdisabled";
            public const string IsAdmin = "isadmin";
            public const string DomainName = "domainname";
            public const string AccountName = "samaccountname";
        }
        public UserIdentityEntity()
        {
            this.LogicalName = "useridentity";
        }
        //public Dictionary<string, object> Attributes { get; protected set; } = new Dictionary<string, object>();

        //public T GetAttributeValue<T>(string key) => this.Attributes.GetValue<T>(key);
        //public void SetAttributeValue<T>(string key, T value) => this.Attributes.AddOrUpdate<T>(key, value);

        public string[] GroupNames => this.GetAttributeValue<string[]>(Schema.GroupNames) ?? new string[] { };

        public string Title { get => this.GetAttributeValue<string>(Schema.Title); set => this.SetAttributeValue(Schema.Title, value); }
        public string UserName { get => this.GetAttributeValue<string>(Schema.UserName); set => this.SetAttributeValue(Schema.UserName, value); }
        public string AccountName { get => this.GetAttributeValue<string>(Schema.AccountName); set => this.SetAttributeValue(Schema.AccountName, value); }

        public string DisplayName { get => this.GetAttributeValue<string>(Schema.DisplayName); set => this.SetAttributeValue(Schema.DisplayName, value); }
        public string Email { get => this.GetAttributeValue<string>(Schema.Email); set => this.SetAttributeValue(Schema.Email, value); }
        public string IpPhoneExtension { get => this.GetAttributeValue<string>(Schema.IpPhoneExtension); set => this.SetAttributeValue(Schema.IpPhoneExtension, value); }

        public bool IsDisabled { get => this.GetAttributeValue<bool>(Schema.IsDisabled); set => this.SetAttributeValue(Schema.IsDisabled, value); }
        public bool IsAdmin { get => this.GetAttributeValue<bool>(Schema.IsAdmin); set => this.SetAttributeValue(Schema.IsAdmin, value); }
        public string DomaiName { get => this.GetAttributeValue<string>(Schema.DomainName); set => this.SetAttributeValue(Schema.DomainName, value); }

        public string Token { get => this.GetAttributeValue<string>("token"); set => this.SetAttributeValue("token", value); }
        
        public string UserPrincipalName
        {
            get
            {
                return this.UserName;
                //return !string.IsNullOrWhiteSpace(this.DomaiName)
                //    ? $"{this.AccountName.Trim()}@{this.DomaiName.Trim()}"
                //    : this.Email;
            }
        }
        public void AddGroupNames(params string[] groupNames)
        {
            var names = new List<string>();
            names.AddRange(this.GroupNames);
            foreach (var grp in groupNames)
            {
                if (!names.Contains(grp))
                    names.Add(grp);
            }
            this.SetAttributeValue(Schema.GroupNames, names.ToArray());
            //this.Attributes.AddOrUpdate(Schema.GroupNames, names.ToArray());
        }

        public ClaimsIdentity GetClaimsIdentity()
        {
            var claims = new GenericIdentity(this.UserName);
            if (!string.IsNullOrWhiteSpace(this.UserPrincipalName))
            {
                claims.AddClaim(new Claim(ClaimTypes.Upn, this.UserPrincipalName));
            }

            if (!string.IsNullOrWhiteSpace(this.Email))
            {
                claims.AddClaim(new Claim(ClaimTypes.Email, this.Email));
            }

            if (!string.IsNullOrWhiteSpace(this.Title))
            {
                claims.AddClaim(new Claim(ClaimTypes.Role, this.Title));
            }
            if (!string.IsNullOrWhiteSpace(this.DisplayName))
            {
                claims.AddClaim(new Claim(ClaimTypesEx.DisplayName, this.DisplayName));
            }
            if (!string.IsNullOrWhiteSpace(this.IpPhoneExtension))
            {
                claims.AddClaim(new Claim(ClaimTypesEx.IpPhone, this.IpPhoneExtension));
            }
            if (this.IsAdmin)
            {
                claims.AddClaim(new Claim(ClaimTypes.Role, "admin"));
            }
            return claims;
        }

        public string GetPreWindows2000UserName()
        {
            var result = this.UserName;
            if (!string.IsNullOrWhiteSpace(result) && result.Contains("@"))
            {
                result = result.ToLowerInvariant();
                result = $"{result.Split('@')[1].Split('.')[0]}\\{result.Split('@')[0]}";
            }
            return result;
        }

        

    }
}
