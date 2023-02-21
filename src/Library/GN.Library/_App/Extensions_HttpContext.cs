using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library
{
	class MyHttpContext
	{
		private static IHttpContextAccessor m_httpContextAccessor;
		public static HttpContext Current => m_httpContextAccessor.HttpContext;
		public static string AppBaseUrl => $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}";
		internal static void Configure(IHttpContextAccessor contextAccessor)
		{
			m_httpContextAccessor = contextAccessor;
		}
	}
	static class HttpContextExtensions
	{
		public static IApplicationBuilder UseHttpContext(this IApplicationBuilder app)
		{
			MyHttpContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
			return app;
		}
		public static Task<HttpResponseMessage> PostAsJsonAsync<T>(
			this HttpClient httpClient, string url, T data)
		{
			var dataAsString = JsonConvert.SerializeObject(data);
			var content = new StringContent(dataAsString);
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			return httpClient.PostAsync(url, content);
		}

		public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
		{
			var dataAsString = await content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<T>(dataAsString);
		}



	}
}
