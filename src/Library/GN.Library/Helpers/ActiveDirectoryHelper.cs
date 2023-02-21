using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Text;
using System.Linq;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using GN.Library.Shared.Internals;

namespace GN.Library.Helpers
{

    /// <summary>
    /// Provides a set of static functions to interact with Active Directory.
    /// </summary>
    /// <remarks>
    /// References:
    /// https://www.codemag.com/article/1312041/Using-Active-Directory-in-.NET
    /// </remarks>

    public static class ActiveDirectoryHelper
    {
        internal class NormalizedUserName
        {
            public string UserName { get; set; }
            public string DomianName { get; set; }
        }

        /// <summary>
        /// List of known active directory attributes.
        /// </summary>
        /// http://www.kouti.com/tables/userattributes.htm
        public class Schema
        {
            public const string SamAccountName = "samaccountname";
            public const string Mail = "mail";
            public const string Mobile = "mobile";
            public const string UserGroup = "usergroup";
            public const string DisplayName = "displayname";
            public const string Title = "title";
            public const string Department = "department";
            public const string IpPhone = "ipphone";
            public const string IsDeleted = "isdeleted";
            public const string UserAccountControl = "useraccountcontrol";
            public const string UserPrincipalName = "userprincipalname";
            public const string MemberOf = "memberof";
        }
        internal static string[] DefualtProperties =>
             new string[]
            {
                "samaccountname","mail","mobile","usergroup","displayname","Title","department","ipPhone",
                "isDeleted","userAccountControl","userPrincipalName","memberOf"
            };
        internal static Tuple<string, string> GetUserAndDomainName(string userName)
        {
            userName = userName ?? "";
            var domainName = GetCurrentDomainName();
            if (userName.Contains("\\"))
            {
                domainName = userName.Split('\\')[0];
                userName = userName.Split('\\')[1];
            }
            else if (domainName.Contains('@'))
            {
                domainName = userName.Split('@')[1];
                userName = userName.Split('@')[0];

            }
            return new Tuple<string, string>(domainName, userName);
        }
        public static string GetCurrentDomainName(string userName = null, string password = null)
        {
            var result = GetCurrentDomainPath(userName, password);
            if (result != null)
            {
                result = result
                    .Replace("LDAP://", "")
                    .Replace("DC=", "")
                    .Replace(",", ".");

            }
            return result;
        }
        /// <summary>
        /// Returns LDAP path to current defialt domain. 
        /// </summary>
        /// <returns></returns>

        public static string GetCurrentDomainPath(string userName = null, string password = null)
        {
            DirectoryEntry de = string.IsNullOrWhiteSpace(userName)
                ? new DirectoryEntry("LDAP://RootDSE")
                : new DirectoryEntry("LDAP://RootDSE", userName, password);
            return "LDAP://" + de.Properties["defaultNamingContext"][0].ToString();
        }
        //internal static ActiveDirectoryEntity ToActiveDirectoryEntity(this SearchResult sr)
        //{
        //    ActiveDirectoryEntity result = null;
        //    try
        //    {
        //        if (sr != null)
        //        {
        //            result = new ActiveDirectoryEntity();
        //            foreach (var _propName in sr.Properties.PropertyNames)
        //            {
        //                if (_propName != null)
        //                {
        //                    var p = _propName.ToString();
        //                    if (sr.Properties.Contains(p))
        //                    {
        //                        var values = sr.Properties[p];
        //                        if (values.Count == 1)
        //                        {
        //                            result.SetAttributeValue(p, values[0]);
        //                        }
        //                        else if (values.Count > 1)
        //                        {
        //                            var comma_seperated_values = "";
        //                            foreach (var value in values)
        //                            {
        //                                if (value != null)
        //                                {
        //                                    comma_seperated_values = comma_seperated_values + (comma_seperated_values == "" ? "" : ";") + value?.ToString();
        //                                }
        //                            }

        //                            result.SetAttributeValue(p, comma_seperated_values);
        //                        }
        //                        else
        //                        {
        //                            result.SetAttributeValue(p, null);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //    return result;
        //}

        internal static Dictionary<string, string> ToDictionary(this SearchResult sr)
        {
            Dictionary<string, string> result = null;
            try
            {
                if (sr != null)
                {
                    result = new Dictionary<string, string>();
                    foreach (var _propName in sr.Properties.PropertyNames)
                    {
                        if (_propName != null)
                        {
                            var p = _propName.ToString();
                            if (sr.Properties.Contains(p))
                            {
                                var values = sr.Properties[p];
                                if (values.Count == 1)
                                {
                                    result.Add(p, values[0].ToString());
                                }
                                else if (values.Count > 1)
                                {
                                    var comma_seperated_values = "";
                                    foreach (var value in values)
                                    {
                                        if (value != null)
                                        {
                                            comma_seperated_values = comma_seperated_values + (comma_seperated_values == "" ? "" : ";") + value?.ToString();
                                        }
                                    }

                                    result[p] = comma_seperated_values;
                                }
                                else
                                {
                                    result[p] = null;
                                }
                            }
                        }
                    }
                }

            }
            catch
            {
                return null;
            }
            return result;
        }

        internal static DirectorySearcher AddProperties(this DirectorySearcher ds, string[] props)
        {
            props = props ?? DefualtProperties;
            props.ToList()
                    .ForEach(x =>
                    {
                        if (!ds.PropertiesToLoad.Contains(x))
                        {
                            ds.PropertiesToLoad.Add(x);
                        }
                    });
            return ds;
        }
        public static IEnumerable<string> GetAllDomainPathes(string userName, string password)
        {
            List<string> result = new List<string>();
            DirectoryContext ctx = new DirectoryContext(DirectoryContextType.Forest, userName, password);
            var forest = Forest.GetForest(ctx);
            var domains = forest.Domains;
            for (var i = 0; i < domains.Count; i++)
            {
                result.Add(domains[i].GetDirectoryEntry().Path);
            }
            return result;
        }
        internal static IEnumerable<Domain> GetAllDomains(string userName, string password)
        {
            List<Domain> result = new List<Domain>();
            DirectoryContext ctx = string.IsNullOrWhiteSpace(userName)
                ? new DirectoryContext(DirectoryContextType.Forest)
                : new DirectoryContext(DirectoryContextType.Forest, userName, password);
            var forest = Forest.GetForest(ctx);
            var domains = forest.Domains;
            for (var i = 0; i < domains.Count; i++)
            {
                result.Add(domains[i]);
            }
            return result;
        }
        /// <summary>
        /// Returns the list of users as a property bag dictionary by traversing all forest domains.
        /// 
        /// </summary>
        /// <param name="userName">Username that is used to connect to domain controller.</param>
        /// <param name="password">Password used to connect to domain controller.</param>
        /// <returns></returns>
        public static IEnumerable<Dictionary<string, string>> GetUsers(string userName = null, string password = null)
        {
            var result = new List<Dictionary<string, string>>();
            GetAllDomains(userName, password)
                .Select(x => x?.GetDirectoryEntry()?.Path)
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList()
                .ForEach(x => result.AddRange(GetDomainUsers(x, userName, password)));
            return result;

        }
        internal static string GetPathFromDomianName(string domainName)
        {
            var result = domainName;
            if (!domainName.ToLowerInvariant().Contains("ldap://"))
            {
                result = $"LDAP://{domainName}";
            }
            return result;
        }
        /// <summary>
        /// Returns all users in a domain path or the default domain. Use domain name, e.g. gnco.local
        /// or a domain path such as 'LDAP://gnco.local, CN=gnco,CN=local'. Use name and passwords
        /// are only used to open the connection to domain controller. It can be used with defalut 
        /// parameters to get all users of the defualt domain using the current user credetntials.
        /// The return value is an array of dictionaries each, corresponds to a user properties.
        /// 
        /// </summary>
        /// <param name="domainNameOrPath">Domain name, like gnco.local or ldap path such as LDAP://gnco.local, DC=gnoc,DC=local. 
        /// If null the default domain will be used</param>
        /// <param name="userName">Username to connect to domain controller.</param>
        /// <param name="password">Password to connect to domain controller.</param>
        /// <returns></returns>
        public static IEnumerable<Dictionary<string, string>> GetDomainUsers(string domainNameOrPath = null, string userName = null, string password = null)
        {
            SearchResultCollection results;
            domainNameOrPath = domainNameOrPath ?? GetCurrentDomainPath();
            domainNameOrPath = GetPathFromDomianName(domainNameOrPath);
            var de = string.IsNullOrWhiteSpace(userName)
                ? new DirectoryEntry(domainNameOrPath)
                : new DirectoryEntry(domainNameOrPath, userName, password);
            var ds = new DirectorySearcher(de);//.AddProperties(DefualtProperties);

            ds.SizeLimit = 0;
            ds.PageSize = 500;

            ds.Filter = "(&(objectCategory=User)(objectClass=person))";
            results = ds.FindAll();
            return results == null
                ? new Dictionary<string, string>[] { }
                : results.OfType<SearchResult>()
                .Select(x => x.ToDictionary())
                .Where(x => x != null)
                .ToArray();
        }


        internal static string GetPropertyValue(this SearchResult sr, string propertyName)
        {
            string ret = string.Empty;

            if (sr.Properties[propertyName].Count > 0)
                ret = sr.Properties[propertyName][0].ToString();

            return ret;
        }
        public static Dictionary<string, string> GetUser(string userName, string ldapPath = null, string adminName=null, string adminpassword=null)
        {
            DirectorySearcher ds = null;
            //if (string.IsNullOrWhiteSpace(ldapPath))
            //{
            //    ldapPath = GetUserAndDomainName(userName).Item1;
            //}
            try
            {
                ldapPath = ldapPath ?? GetUserAndDomainName(userName).Item1;
                userName = GetUserAndDomainName(userName).Item2;
                ldapPath = GetPathFromDomianName(ldapPath);


                DirectoryEntry de = new DirectoryEntry(ldapPath,adminName, adminpassword);
                SearchResult sr;
                ds = new DirectorySearcher(de);
                // Set the filter to look for a specific user
                ds.Filter = "(&(objectCategory=User)(objectClass=person)(samaccountname=" + userName + "))";
                sr = ds.FindOne();
                return sr.ToDictionary();
            }
            catch { }
            return null;
        }

        public static bool AuthenticateUser(string userName, string password, string domainName = null)
        {
            return AuthenticateBySearch(userName, password, domainName) || AuthenticateByValidate(userName, password, domainName);
        }

        public static bool AuthenticateBySearch(string userName, string password, string domainName = null)
        {
            bool ret = false;
            try
            {
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    domainName = GetUserAndDomainName(userName).Item1;
                    userName = GetUserAndDomainName(userName).Item2;
                }
                DirectoryEntry de = new DirectoryEntry("LDAP://" + domainName, userName, password);
                DirectorySearcher dsearch = new DirectorySearcher(de);
                SearchResult results = null;
                results = dsearch.FindOne();
                ret = true;
            }
            catch
            {
                ret = false;
            }
            return ret;
        }
        public static bool AuthenticateByValidate(string userName, string pass, string domainName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    domainName = GetUserAndDomainName(userName).Item1;
                }
                //if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(pass))
                //    return false;
                using (var pc = new PrincipalContext(ContextType.Domain, domainName))
                {
                    return pc.ValidateCredentials(userName, pass, ContextOptions.SimpleBind);
                }
            }
            catch
            {
            }
            return false;
        }
        internal static NormalizedUserName NormalizeUserName(string userName)
        {
            NormalizedUserName result = new NormalizedUserName()
            {
                UserName = userName
            };
            if (userName == null)
            {
                return null;
            }
            if (userName.Contains("@"))
            {
                var parts = userName.Split('@');
                result.UserName = parts[0];
                result.DomianName = parts[1];
            }
            else if (userName.Contains("\\"))
            {
                var parts = userName.Split('\\');
                result.UserName = parts[1];
                result.DomianName = parts[0];
            }
            else
            {

            }
            if (result.DomianName != null)
            {
                result.DomianName = result.DomianName.ToLowerInvariant();
            }
            return result;

        }

        internal static string ValueOrNull(this IDictionary<string, string> dic, string key)
        {
            return dic.TryGetValue(key, out var res) ? res : null;
        }
        public static UserIdentityEntity FromActiveDirectoryAttributes(IDictionary<string, string> attributes)
        {
            if (attributes == null)
                return null;
            var result = new UserIdentityEntity();
            foreach (var k in attributes)
            {
                result.SetAttributeValue(k.Key, k.Value);
            }
            result.UserName = attributes.ValueOrNull(Schema.UserPrincipalName);
            result.UserName = result.UserName?.ToLowerInvariant();
            result.DisplayName = attributes.ValueOrNull(Schema.DisplayName);
            result.IsAdmin = result.GroupNames.Contains("Domain Admins");
            result.Title = attributes.ValueOrNull(Schema.Title);
            result.IsDisabled = result.GetAttributeValue<int>(Schema.UserAccountControl) == 514 || result.GetAttributeValue<int>(Schema.UserAccountControl) == 66050;
            //public bool IsDisabled => (new int[] { 514, 66050 }).Contains(GetAttributeValue<int>(Schema.UserAccountControl));
            //        UserName = acUser.LogonName?.ToLowerInvariant(),
            //DisplayName = acUser.DisplayName,
            //IpPhoneExtension = acUser.Extension,
            //Email = acUser.Mail,
            //IsDisabled = acUser.IsDisabled,
            //Title = acUser.Title,
            //IsAdmin = acUser.IsAdmin,
            //DomaiName = acUser.DomainName,
            //AccountName = acUser.AccountName

            return result;
        }
    }
}
