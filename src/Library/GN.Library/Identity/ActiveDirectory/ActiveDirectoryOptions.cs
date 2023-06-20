using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Identity.ActiveDirectory
{
	public class ActiveDirectoryOptions
	{
		const string DefaultPropertyNames = "samaccountname,mail,displayname,title,personalTitle,department,usergroup";
		public bool Disabled { get; set; }
		public ActiveDirectoryOptions()
		{
			this.PropertyNames = DefaultPropertyNames;
		}

		public string AdminUserName { get; set; }
		public string AdminPassword { get; set; }

		public string LDAPServerName { get; set; }
		public string DefaultDomainName { get; set; }

		public string PropertyNames { get; set; }

		public override string ToString()
		{
			return $"ActiveDirectoryOptions UserName:'{AdminUserName}'";
		}

	}
}
