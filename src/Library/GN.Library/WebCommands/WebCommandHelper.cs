using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.WebCommands
{
	public class WebCommandHelper
	{
		public static void  Call(string url)
		{
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(url);
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));
			var ping = new PingCommand();
			try
			{
				//client.PostAsJsonAsync
				var f = client.PostAsJsonAsync("api/xrmapi2", ping).ConfigureAwait(false).GetAwaiter().GetResult();
			}
			catch (Exception err)
			{
				var exp = err;
			}
			//return true;


		}
	}
}
