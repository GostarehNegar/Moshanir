using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Xrm
{
	public class CrmUriHelper : Uri
	{
		public CrmUriHelper(string path, string organizationName)
			: base(path)
		{
			this.OrganizationName = !string.IsNullOrWhiteSpace(organizationName)
				? organizationName
				: GetOrganizationNameFromUri(this);
		}
		public string OrganizationName { get; private set; }
		public bool HasOrganizationName { get { return !string.IsNullOrEmpty(OrganizationName); } }
		public static string GetOrganizationNameFromUri(Uri uri)
		{
			return uri == null
				? null
				: uri.Segments.Length < 2
					? null
					: string.IsNullOrEmpty(uri.Segments[1])
						? null
						: uri.Segments[1].ToLowerInvariant().StartsWith("xrmservices")
							? null
							: uri.Segments[1].ToLowerInvariant().EndsWith("/")
								? uri.Segments[1].Substring(0, uri.Segments[1].Length - 1)
								: uri.Segments[1];
		}
		public CrmUriHelper GetOrganizationServiceUri()
		{

			var str = this.Port != 0
				? string.Format("{0}://{1}:{2}/{3}/XRMServices/2011/Organization.svc", this.Scheme, this.Host, this.Port, this.OrganizationName)
				: string.Format("{0}://{1}/{2}/XRMServices/2011/Organization.svc", this.Scheme, this.Host, this.OrganizationName);

			return new CrmUriHelper(str, null);

		}
		public CrmUriHelper GetWebApiServiceUrl(int version = 8)
		{
			var str = this.Port != 0
				? string.Format("{0}://{1}:{2}/{3}/api/data/v8.0/", this.Scheme, this.Host, this.Port, this.OrganizationName)
				: string.Format("{0}://{1}/{2}/api/data/v8.0/", this.Scheme, this.Host, this.OrganizationName);
			return new CrmUriHelper(str, null);

		}
		public CrmUriHelper GetDiscoveryServiceUri()
		{
			var str = this.Port != 0
				? string.Format("{0}://{1}:{2}/XRMServices/2011/Discovery.svc", this.Scheme, this.Host, this.Port)
				: string.Format("{0}://{1}/XRMServices/2011/Discovery.svc", this.Scheme, this.Host);
			return new CrmUriHelper(str, OrganizationName);
		}
		public CrmUriHelper GetDeploymentServiceUri()
		{
			var str = this.Port != 0
				? string.Format("{0}://{1}:{2}/XRMServices/2011/Delpoyment.svc", this.Scheme, this.Host, this.Port)
				: string.Format("{0}://{1}/XRMServices/2011/Deployment.svc", this.Scheme, this.Host);
			return new CrmUriHelper(str, OrganizationName);
		}
		public static CrmUriHelper TryCreate(string path, string organizationName)
		{
			try
			{
				return new CrmUriHelper(path, organizationName);
			}
			catch { }
			return null;

		}
	}
}
