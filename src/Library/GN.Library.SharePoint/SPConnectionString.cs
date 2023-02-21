using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.SharePoint
{
    public class SPConnectionString
    {
        public string Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }

        public SPConnectionString WithUrl(string url)
        {
            this.Url = url;
            return this;
        }
        internal NetworkCredential GetCredentials()
        {
            return string.IsNullOrEmpty(UserName)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(this.UserName, this.Password, this.Domain);
        }
        public static SPConnectionString Parse(string connectionString)
        {
            var result = new SPConnectionString();
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                if (!connectionString.Contains("="))
                {
                    connectionString = "Url=" + connectionString;
                }
                var values = SharePointExtensions.ParseConnectionString(connectionString);
                string getValue(string s)
                {
                    if (values.Any(x => string.Compare(x.Key, s, true) == 0))
                    {
                        return values.FirstOrDefault(x => string.Compare(x.Key, s, true) == 0).Value;
                    }
                    return null;
                }
                result.Url = getValue("Url");
                result.UserName = getValue("UserName");
                result.Password = getValue("Password");
                result.Domain = getValue("Domain");
            }
            return result;
        }
    }
}
