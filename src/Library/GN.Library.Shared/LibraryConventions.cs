using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GN.Library.Shared;
using System.Security.Claims;
using GN.Library.Shared.Entities;

namespace GN.Library
{
    public interface ILibraryConventions
    {
        bool IsValidTokenSyntax(string token);
        bool IsValidUserPrincipalName(string upn);
        bool IsValidEndpointName(string endpoint);
        string GGGG(string userId);
        bool IsStreamNameValid(string stream);
        bool TryGetUserIdFromClaimsPrincipal(ClaimsPrincipal claims, out string userId);
        string Notifications { get; }
        string GetNotificationTopic(System.Security.Claims.ClaimsPrincipal principal);
        string RawSip { get; }
        string SipRawForChannel(string channel = "*");
        string UserIdToDomainName(string userId);
        string LoginNameToUserId(string logInName);

    }

    public class LibraryConventions : ILibraryConventions
    {
        public class Constants : LibraryConstants { }
        public static LibraryConventions Instance { get; private set; } = new LibraryConventions();

        public bool IsValidTokenSyntax(string token)
        {
            return !string.IsNullOrWhiteSpace(token) && token.Length > 10;
        }
        public bool IsValidUserPrincipalName(string upn)
        {
            return !string.IsNullOrEmpty(upn);
        }
        /// <summary>
        /// Returns true if the supplied name is valid as an endpoint name.
        /// Endpoint names are used in message bus to identity messaging end points.
        /// Each messagebus instance is considered an endpoint.
        /// This is sepcsially used in EventHub to validate a new subscribing
        /// endpoint.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public bool IsValidEndpointName(string endpoint)
        {
            return !string.IsNullOrWhiteSpace(endpoint) && endpoint.Length > 3;
        }
        public string GGGG(string userId)
        {
            return userId.ToLowerInvariant();
        }
        public bool IsStreamNameValid(string stream)
        {
            return !string.IsNullOrWhiteSpace(stream) && !stream.Contains("*");
        }
        public bool TryGetUserIdFromClaimsPrincipal(ClaimsPrincipal claims, out string userId)
        {
            var result = false;
            userId = null;
            if (!string.IsNullOrWhiteSpace(claims?.Identity?.Name))
            {
                userId = claims.Identity.Name.ToLowerInvariant();
                result = true;
            }
            return result;
        }

        public bool TryGetUserIdFromClaimsIdentity(ClaimsIdentity claims, out string userId)
        {
            var result = false;
            userId = null;
            if (!string.IsNullOrWhiteSpace(claims.Name))
            {
                userId = claims.Name.ToLowerInvariant();
                result = true;
            }
            return result;
        }

        public string Notifications => "Notifications";
        public string GetUserNameForTopic(string userName)
        {
            return userName?.ToLowerInvariant();
        }
        public string GetNotificationTopic(System.Security.Claims.ClaimsPrincipal principal)
        {
            return $"{Notifications}.{GetUserNameForTopic(principal?.Identity?.Name)}";
        }
        public string GetNotificationTopic(string userName, string topic = "message")
        {
            return $"{Notifications}.{GetUserNameForTopic(userName)}.topic";
        }
        public string RawSip => Constants.SipRawTopic;
        public string SipRawForChannel(string channel = "*") => $"{Constants.SipRawTopic}/{channel}";

        public Tuple<string, string> NormalizeUserName(string userName)
        {
            var domain = "";
            var user = "";
            if (userName == null)
            {
                return new Tuple<string, string>(user, domain);
            }
            if (userName.Contains("@"))
            {
                var parts = userName.Split('@');
                user = parts[0].ToLowerInvariant();
                domain = parts[1].ToLowerInvariant();
            }
            else if (userName.Contains("\\"))
            {
                var parts = userName.Split('\\');
                user = parts[1].ToLowerInvariant();
                domain = parts[0].ToLowerInvariant();
            }
            else
            {

            }
            return new Tuple<string, string>(user, domain);

        }


        public string LoginNameToUserId(string logInName)
        {
            var dname = (LibraryConstants.DomianName ?? "")
                .Split('.')
                .LastOrDefault();
            if (logInName.Contains('\\'))
            {
                var splitted = logInName.Split('\\');
                return (splitted[1] + "@" + splitted[0] + "." + dname).ToLowerInvariant();
            }
            throw new Exception("invalid login name");
        }

        public string UserIdToDomainName(string userId)
        {
            if (userId.Contains('@') && userId.Contains('.'))
            {
                var splitted = userId.Split('@');
                var domainName = splitted[1].Split('.')[0];
                return $"{domainName}\\{splitted[0]}";
            }
            else
            {
                throw new Exception("invalid userId");
            }
        }

        public string ValidateSignalRTransportConnectionString(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                url = "http://localhost:5000";
            }
            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Host}" + (uri.Port == 0 ? "" : $":{uri.Port}") + LibraryConventions.Constants.MessageBusHubUrl;
        }
        public bool IsActivity(DynamicEntity entity)
        {
            return entity != null && entity.Attributes != null && entity.Attributes.ContainsKey("acitvityid");
        }
    }
}
