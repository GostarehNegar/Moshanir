using System;

namespace GN.Library.Shared
{
	[AttributeUsage(AttributeTargets.Class ,AllowMultiple = true, Inherited = true)]
	public class AuthorizeMessageAttribute: Attribute
	{
		//
		// Summary:
		//     Gets or sets a comma delimited list of roles that are allowed to access the resource.
		public string Roles { get; set; }
	}
}
