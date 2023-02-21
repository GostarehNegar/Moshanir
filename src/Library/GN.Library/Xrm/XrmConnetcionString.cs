using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GN.Library.Helpers;
using Microsoft.Extensions.Configuration;
using GN.CodeGuard;
using GN.Library.Configurations;

namespace GN.Library.Xrm
{
	/// <summary>
	/// A class for parsing connection strings for parameters required to create
	/// 'Crm Organization services'. These connection strings would look like:
	/// [add name="Xrm" connectionString="Url=http://gndcrm2016b/gndev; Domain=gnco; Username=babak; Password=***;"/]
	/// Refer to https://msdn.microsoft.com/en-us/library/gg695805%28v=crm.7%29.aspx for more information on Xrm Organization configuration.
	/// The class may alsoe be used to manually configure connection parameters.
	/// </summary>
	public class XrmConnectionString
	{
		private string userName;
		private static string connectionString;
		/// <summary>
		/// User name for connection use null for connection with
		/// current user privilege. 
		/// </summary>
		public string UserName
		{
			get { return userName; }
			set
			{
				if (value != null && value.Contains("\\"))
				{
					userName = value.Split('\\')[1];
					DomainName = value.Split('\\')[0];
				}
				else
					userName = value;
			}
		}
		/// <summary>
		/// Url of the orgnization e.g. "http://SERVER_NAME/ORGANIZATON_NAME".
		/// </summary>
		public string Url { get; set; }
		/// <summary>
		/// Domain name for stablising full user name for authentication.
		/// </summary>
		public string DomainName { get; set; }
		/// <summary>
		/// User password in stablishing the connection. 
		/// Cann be null for current user authentication.
		/// </summary>
		public string Password { get; set; }
		/// <summary>
		/// Rwa connection string that is get from settings.
		/// </summary>
		public string RawConnectionString => connectionString;
		/// <summary>
		/// Connection string based on current parameters.
		/// </summary>
		public string ConnectionString => BuildConnetionString();

		public CrmUriHelper Uri
		{
			get
			{
				CrmUriHelper result = null;
				try
				{
					result = new CrmUriHelper(this.Url, null);
				}
				catch { }
				return result;
			}
		}

		/// <summary>
		/// Builds a connection string based on current parameterd.
		/// </summary>
		/// <returns></returns>
		/// 
		public string BuildConnetionString(string url = null, string userName = null, string domainName = null, string password = null)
		{
			return $"Url={url ?? Url}; Username={userName ?? UserName}; Domain={domainName ?? DomainName}; Password={password ?? Password}";
		}
		/// <summary>
		/// Returns the Organization service Uri e.g. http://server/organization//XrmServices/2011/Organization.Svc
		/// </summary>
		public Uri OrganizationServiceUri => Uri?.GetOrganizationServiceUri();

		public Uri WebApiUri => Uri?.GetWebApiServiceUrl();

		public static IEnumerable<KeyValuePair<string, string>> ParseConnectionString(string connectionString)
		{
			var result = new List<KeyValuePair<string, string>>();
			if (!string.IsNullOrWhiteSpace(connectionString))
			{
				foreach (var item in connectionString.Split(';'))
				{
					var keyValueArray = item.Split('=');
					var key = keyValueArray[0];
					var val = keyValueArray.Length > 1 ? keyValueArray[1] : null;
					if (!string.IsNullOrEmpty(key))
						result.Add(new KeyValuePair<string, string>(key.Trim(), val?.Trim()));
				}
			}
			return result;
		}
		/// <summary>
		/// Reset the connection string based on suppplied parameters.
		/// If any parameter is null the current values of that parameter is
		/// retained anf not overrided. Use blank strings to clear old values.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <param name="domainName"></param>
		public XrmConnectionString Reset(string url, string userName, string password, string domainName)
		{
			return Reset(BuildConnetionString(url, userName, domainName, password));
		}

		/// <summary>
		/// Resets connection string with the supplied one.
		/// </summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		public XrmConnectionString Reset(string connectionString)
		{
			Guard.That(connectionString, nameof(connectionString)).IsNotNull();
			if (!string.IsNullOrWhiteSpace(connectionString))
			{
				var values = ParseConnectionString(connectionString);
				string getValue(string s)
				{
					if (values.Any(x => string.Compare(x.Key, s, true) == 0))
					{
						return values.FirstOrDefault(x => string.Compare(x.Key, s, true) == 0).Value;
					}
					return null;
				}
				this.DomainName = null;
				this.UserName = getValue("username")?.Trim();
				this.Url = getValue("url")?.Trim();
				this.DomainName = this.DomainName ?? getValue("domain")?.Trim();

				this.Password = getValue("password")?.Trim();
			}

			return this;

		}
		public XrmConnectionString(IAppConfiguration settings)
		{
			if (connectionString == null && settings != null)
			{
				connectionString = connectionString ?? settings.GetRawXrmConnectionString();
				connectionString = connectionString ?? "";
				Reset(connectionString);
			}
		}

		public override string ToString()
		{
			return this.ConnectionString;
		}
		public XrmConnectionString Clone()
		{
			return new XrmConnectionString(null).Reset(this.BuildConnetionString());
		}
	}


}
