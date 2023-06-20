using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Web;
using NLog;
using System.Reflection;
using System.Diagnostics;

namespace GN.Library
{

	public class NullStartup
	{
		public static IConfiguration Configuration { get; protected set; }
		public NullStartup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder();
				//.SetBasePath(env == null ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) : env.ContentRootPath)
				//.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				//.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				//.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libsettings.json"), optional: true)
				//.AddEnvironmentVariables();
			Configuration = builder.Build();
		}
		public virtual void ConfigureServices(IServiceCollection services)
		{
		}
		public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
		}
	}

	public class AppStartup
	{
		public static IConfiguration Configuration { get; protected set; }
		public AppStartup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env == null ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) : env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libsettings.json"), optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}
		public virtual void ConfigureServices(IServiceCollection services)
		{
			services.AddGNLib(Configuration, cfg =>
			{
			});
			//services.AddMvc();
		}
		public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
			var pathToContentRoot = Path.GetDirectoryName(pathToExe);
			//var cfg = app.ApplicationServices.GetServiceEx<IAppConfiguration>();
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			//app.UseMvc();
			//app.UseStaticFiles();
			app.UseGNLib();
		}
	}

	//public class AppStartup_Deprecated
	//{
	//	public IHostingEnvironment HostingEnvironment { get; protected set; }
	//	public static IConfiguration Configuration { get; protected set; }
	//	public static IAppConfiguration AppConfiguration { get; protected set; }
	//	public IServiceCollection ServiceDescriptors { get; protected set; }
	//	//public static Action<IAppConfiguration> Configurator;

	//	public AppStartup_Deprecated(IHostingEnvironment env)
	//	{
	//		var builder = new ConfigurationBuilder()
	//			.SetBasePath(env == null ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) : env.ContentRootPath)
	//			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	//			.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
	//			.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libsettings.json"), optional: true)
	//			.AddEnvironmentVariables();
	//		Configuration = builder.Build();
	//		//AppConfiguration = new AppConfiguration(null, Configuration);
	//	}
	//	//public AppStartup() { }
	//	// This method gets called by the runtime. Use this method to add services to the container.
	//	// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
	//	public virtual void ConfigureServices(IServiceCollection services)
	//	{
	//		var options = AppBuildOptions.Current;
	//		ServiceDescriptors = services;
	//		//Configurator?.Invoke(AppConfiguration);
	//		services.AddSingleton<IApplicationLifetime, Microsoft.AspNetCore.Hosting.Internal.ApplicationLifetime>();
	//		services.AddLogging(x =>
	//		{
	//			x.SetMinimumLevel(options.MimimumLogLevel);
	//			x.AddDebug();
	//		});
	//		//services
	//			//.AddMvc()
	//			//.AddApplicationPart(typeof(GN.AppHost).Assembly);
	//		services.AddLibraryServices();

	//		//var modules = AppHost_Deprectated.Modules.GetAll();
	//		//foreach (var module in modules)
	//		//{
	//		//	module.AddServices(services);
	//		//}
	//	}


	//	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	//	public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
	//	{
	//		var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
	//		var pathToContentRoot = Path.GetDirectoryName(pathToExe);
	//		var cfg = app.ApplicationServices.GetServiceEx<IAppConfiguration>();
	//		if (env.IsDevelopment())
	//		{
	//			app.UseDeveloperExceptionPage();
	//		}
	//		//app.UseMvc();
	//		//app.UseStaticFiles();
	//		app.UseGNLib();
	//		//var modules = AppHost_Deprectated.Modules.GetAll();
	//		//foreach (var module in modules)
	//		//{
	//		//	module.UseServices(app, env);
	//		//}

	//	}

		
	//}
}
