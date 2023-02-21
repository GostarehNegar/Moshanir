using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Security
{

	public interface IApiKeyService
	{
		SecurityRoles GetRole(string key);
	}
	public class ApiKeySevice : IApiKeyService
	{
		public static string ServiceKey = "Gn Service Ai Key";
		public SecurityRoles GetRole(string key)
		{
			return ServiceKey.Equals(key) ? SecurityRoles.AdimnService : SecurityRoles.None;
		}
	}
}
