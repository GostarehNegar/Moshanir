using System;
using GN.Library;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GN.Library.Api
{
	public static partial class Extensions
	{

		public static IServiceCollection AddLibraryApi(this IServiceCollection service)
		{
			return service;
		}
		public static IApplicationBuilder UseLibraryApi(this IApplicationBuilder app)
		{
			//app.UseMvc();
			return app;
		}
		
	}
}
