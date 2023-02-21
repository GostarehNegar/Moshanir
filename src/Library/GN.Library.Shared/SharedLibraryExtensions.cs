using GN.Library.Shared;
using GN.Library.Shared.Chats;
using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace GN.Library
{
    public static partial class SharedLibraryExtensions
    {
        public static long ToUnixTimeMilliseconds(this DateTime date)
        {
            //            return (long) date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            return ((DateTimeOffset)date).ToUnixTimeMilliseconds();

        }
        public static DateTime UtcTimeFromUnixMilliSeconds(long milliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
        }
        public static string GetName(this XrmDynamicEntity entity)
        {
            switch (entity?.LogicalName)
            {
                case ChatUserEntity.Schema.LogicalName:
                    return (entity as ChatUserEntity).FullName;
                case ContactEntity.Schema.LogicalName:
                    return (entity as ContactEntity).FullName;
                case ChatAccountEntity.Schema.LogicalName:
                    return entity.Name;
                default:
                    return entity.Name;
            }
        }
        public static Guid? GetCrmUserId(this ClaimsIdentity identity)
        {
            return identity != null &&
                    !string.IsNullOrEmpty(identity.FindFirst(ClaimTypesEx.CrmUserId)?.Value) &&
                    Guid.TryParse(identity.FindFirst(ClaimTypesEx.CrmUserId)?.Value, out var _res)
                    ? _res
                    : (Guid?)null;
        }
    }
}
