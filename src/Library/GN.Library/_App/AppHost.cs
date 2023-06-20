using GN.Library;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Configuration;
using GN.Library.Messaging;
using GN.Library.Messaging.Internals;
using System.Linq;
using GN.Library.Messaging.Deprecated;
using GN.Library.Shared;



namespace GN
{
	public class AppHost
	{
		private static IHost _host;


		public static void Initialize(IHost host = null, IConfiguration configuration = null, IServiceProvider serviceProvider = null)
        {
            try
            {
				GN.Library.LibraryConstants.DomianName = GN.Library.Helpers.ActiveDirectoryHelper.GetCurrentDomainName();
				Console.WriteLine($"Active Directory DomainName: '{GN.Library.LibraryConstants.DomianName}'");
            }
            catch { }

			if (host != null)
			{
				_host = host;
				serviceProvider = host.Services;
				AppContext.Initialzie(_host.Services);
			}
			if (serviceProvider != null)
			{
				AppContext.Initialzie(serviceProvider);
				configuration = serviceProvider.GetServiceEx<IConfiguration>();
			}
			if (configuration != null && Configuration == null)
			{
				Configuration = configuration;
			}
		}
		//public static IAppHostBuilder Builder => AppHostBuilder.Instance;
		public static IWebHostBuilder GetWebHostBuilder(string[] args = null, Action<AppInfo> configure = null)
		{
			//return WebHost.CreateDefaultBuilder().UseUrlsEx();
			return GetWebHostBuilder<NullStartup>(args, configure);
		}
		public static T GetService<T>()
		{
			return Services.GetService<T>();
		}
		public static IEnumerable<T> GetServices<T>()
		{
			return Services.GetServices<T>();
		}


		public static IWebHostBuilder GetWebHostBuilder<T>(string[] args = null, Action<AppInfo> configure = null) where T : class
		{
			configure?.Invoke(AppInfo.Current);
			AppInfo.Current.Validate();

			var result = WebHost.CreateDefaultBuilder<T>(args);
			result.UseDefaultServiceProvider(s => s.ValidateScopes = false);
			var urlsInCommandLine = args!=null &&  args.Any(x => x != null && x.ToLowerInvariant() == "--urls");
			return urlsInCommandLine ? result : result.UseUrlsEx();
		}
		public static IHostBuilder GetHostBuilder()
		{
			var result = new HostBuilder()
				.UseDefaultServiceProvider(s => s.ValidateScopes = false);
			return result;
		}
		public static IAppContext Context => AppContext.Current;
		public static IAppServices Services => Context.AppServices;

		//public static IAppDataServices DataContext => new AppDataContext(Context);
		public static IAppUtils Utils => new AppUtils();
		//public static IMessageBus_Deprecated_2 Bus_Deprecated => Services.GetService<IMessageBus_Deprecated_2>();
		//public static IMessageBus_Deprecated Bus_deprecated => Services.GetService<IMessageBus_Deprecated>();
		public static IMessageBus Bus => Services.GetService<IMessageBus>();
		public static IProcedureCall Rpc => Services.GetService<IProcedureCall>();
		public static AppInfo AppInfo => AppInfo.Current;
		public static ILibraryConventions Conventions => Services.GetService<ILibraryConventions>();
		public static IConfiguration Configuration { get; private set; }
		internal static bool Initailized => AppContext.IsInitailzied;
		private static ILoggerFactory LoggerFactory
		{
			get
			{
				if (AppContext.IsInitailzied)
					return AppContext.Current.AppServices.GetService<ILoggerFactory>();
				return null;
			}
		}
		//public static GN.ILogger_Deprecated GetLogger<T>()
		//{
		//	return LoggerFacrory.CreateLogger<T>(LoggerFactory);
		//}
		//public static ILogger_Deprecated GetLogger(Type type)
		//{
		//	return LoggerFacrory.CreateLogger(LoggerFactory, type);
		//}

		//public static ILogger GetLoggerEx(Type type)
  //      {
		//	return Services.GetService<ILoggerFactory>().CreateLogger(type);
  //      }
		//public static ILogger GetLoggerEx<T>()
		//{
		//	return Services.GetService<ILoggerFactory>().CreateLogger<T>();
		//}

	}
}
