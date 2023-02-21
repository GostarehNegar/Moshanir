using GN.Library.Shared.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace GN.Library.Authorization
{
	public interface IAuthorizationService
	{
		string GenerateToken(IList<Claim> claims);
		UserLogedInModel Login(string userName, string password, params string[] roles);
		ClaimsPrincipal ValidateToken(string token);
	}
}
