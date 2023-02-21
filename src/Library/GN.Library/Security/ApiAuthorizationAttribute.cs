using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace GN.Library.Security
{
	public class ApiAuthorizationAttribute : Attribute, IAsyncActionFilter
	{
		private SecurityRoles requiredRole;
		public static string ApiKeyHeader = "ApiKey";
		public static string AdminServiceKey = "AdminService";
		public ApiAuthorizationAttribute(SecurityRoles requiredRole)
		{
			this.requiredRole = requiredRole;
		}
		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeader, out var value))
			{
				var role = value.Equals(AdminServiceKey)
					? SecurityRoles.AdimnService
					: SecurityRoles.None;
				if ((role | this.requiredRole) == this.requiredRole)
				{
					await next();
				}
			}
			context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
			return;

		}




	}
}
