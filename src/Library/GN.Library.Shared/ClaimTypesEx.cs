using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace GN.Library.Shared
{

    public class ClaimTypesEx
    {
        public const string Email = ClaimTypes.Email;
        public const string Role = ClaimTypes.Role;
        public const string Upn = ClaimTypes.Upn;
        public const string IpPhone = "http://schemas.microsoft.com/ws/2008/06/identity/claims/ipphone";
        public const string DisplayName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/displayname";
        public const string DomainName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/domainame";
        public const string CrmUserId = "http://schemas.microsoft.com/ws/2008/06/identity/claims/CrmUserId";
    }
}
