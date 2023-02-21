using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Identity
{
	public class TokenOptions
	{
		public string SigningKey { get; set; }
		public bool SkipAuthenticateCommand { get; set; }

		public TokenOptions()
		{
			SigningKey = "gn_portal_signing_key";
		}
		public TokenOptions Validate()
		{
			return this;
		}

	}
}
