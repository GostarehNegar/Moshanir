using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Identity
{
	public interface IAuthenticationProvider
	{
		Task<bool> Authenticate(string userName, string password);
	}
}
