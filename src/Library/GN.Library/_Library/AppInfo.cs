using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using LiteDB;
using System.IO;

namespace GN.Library
{
	public class AppInfo
	{
		[BsonId]
		public string Name { get; set; }
		public string DisplayName { get; set; }
		public static AppInfo Current = new AppInfo();
		public string Urls { get; set; }
		public int Port { get; set; }
		public bool IsMessageServer { get; set; }
		public string ForwardingPatterns { get; set; }

		public AppInfo()
		{
			this.Name = this.Name ?? Path.GetFileName(Environment.GetCommandLineArgs()[0]);
			if (!AppDomain.CurrentDomain.IsDefaultAppDomain())
			{

			}
			this.DisplayName = this.Name;
		}
		public override string ToString()
		{
			return $"Name: '{Name}', Machine: '{Environment.MachineName}'";
		}
		private string fixurl(string url)
        {
			if (url!=null && url.EndsWith("/"))
            {
				return url.Substring(0, url.Length - 1);
            }
			return url;
        }
		public AppInfo Validate()
		{
			this.Name = this.Name ?? Path.GetFileName(Environment.GetCommandLineArgs()[0]);
			this.DisplayName = this.DisplayName ?? this.Name;
			
			//var _urls =GN.Extensions.GetHostingUrls(this.Urls);
			//var uris = GN.Extensions.GetAppUrisEx(null);
			//_urls = AppHost.Utils.GetAppUrls();
			this.Urls = AppHost.Utils.GetAppUris(this.Urls).Select(x=>fixurl(x.AbsoluteUri))
				.Aggregate((current, next) => current + ", " + next);
			//if (AppHost.Initailized)
			//{
			//	var urls = AppHost.Utils.GetAppUrls();
			//	if (urls != null && urls.Length > 0)
			//	{
			//		this.Urls = urls.Aggregate((current, next) => current + ", " + next);
			//	}
			//}
			return this;
		}
		private string[] GetHostingUrls(bool addLocalHost = true)
		{
			var args = Environment.GetCommandLineArgs();
			var result = new List<string>();
			var port = this.Port < 100 ? new Random().Next(2000, 8000) : this.Port;
			foreach (var ip in (this as IAppUtils).GetLocalIPs())
			{
				result.Add($"http://{ip}:{port}");
			}
			if (addLocalHost)
				result.Add($"http://localhost:{port}");
			this.Urls = result.Aggregate((current, next) => current + "," + next);
			return result.ToArray();
		}
		public void Update()
		{
			if (AppHost.Initailized)
			{
				try
				{
					AppHost.GetService<Helpers.IAppServerExplorer>().Update();

				}
				catch { }
			}
		}


	}
}
