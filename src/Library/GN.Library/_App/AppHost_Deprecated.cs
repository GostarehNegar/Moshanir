using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.AspNetCore.Hosting;
using GN.Library;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore;
using Microsoft.Extensions.FileProviders;
using System.IO;
using GN.Library.Configurations;
using AppContext_Deprecated = GN.Library.AppContext_Deprecated;
using Microsoft.AspNetCore.Hosting.Server.Features;
using NLog.Web;
using System.Diagnostics;
using System.Threading;
using GN.Library.Messaging;

namespace GN
{
    /*
	 * 
	 * 
	*/


    /// <summary>
    /// An static helper class to manage application hosting.
    /// Use this class to setup the hosting enviornment bases on 
    /// .Net Core concepts.
    /// </summary>
    public class AppHost_Deprectated
    {

        private static IWebHostBuilder webHostBuilder;
        private static IWebHost webHost;
        private static IAbstactHostBuilder _builder;
        private static IAbstractHost _host;
        private static IAppUtils utils = new AppUtils(AppHost.Context);
        public static bool Initialized { get; private set; }
        public static IWebHostBuilder GetHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args);
        }
        public static IWebHostBuilder GetHostBuilder<T>(string[] args) where T : class
        {
            return WebHost.CreateDefaultBuilder<T>(args)
                .ConfigureAppConfiguration((ctx, cfg) =>
                    {
                        cfg.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libsettings.json"), optional: true);
                    });
        }
        public static IWebHost GetHost()
        {
            return InitializeEx();
        }
        public static IWebHost InitializeEx(IWebHost host = null, IWebHostBuilder builder = null,
            Action<IWebHostBuilder> webHostconfigure = null,
            Action<IAppConfiguration> configure = null,
            Action<AppBuildOptions> buildOptions = null,
            string[] args = null)
        {
            return InitializeEx<AppStartup_Deprecated>(
                host: host, builder: builder, webHostconfigure: webHostconfigure, configure: configure, buildOptions: buildOptions, args: args);
        }

        public static IAbstractHost AbstractInitialize<T>(
            HostTypes type = HostTypes.Web,
            IAbstractHost host = null,
            IAbstactHostBuilder builder = null,
            Action<IAbstactHostBuilder> configureBuilder = null,
            Action<IAppConfiguration> configure = null,
            Action<AppBuildOptions> buildOptions = null, string[] args = null) where T : class
        {
            _host = host ?? _host;
            if (_host == null)
            {
                var options = AppBuildOptions.Current;
                buildOptions?.Invoke(options);
                if (_builder == null)
                {
                    _builder = AbstractHost.CreateDefaultBuilder<T>(options.HostType, args);
                    configureBuilder?.Invoke(_builder);
                    NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
                    if (options.HostType == HostTypes.Generic)
                    {
                        //_builder.HostBuilder.UseNLog();

                    }
                    else
                        _builder.WebHostBuilder.UseNLog();

                    _host = _builder.Build();
                }

            }
            AppContext_Deprecated.Initialize(_host.Services);
            configure?.Invoke(Configuration);
            Initialized = true;

            return _host;
        }

        public static IWebHost InitializeEx<T>(IWebHost host = null,
            IWebHostBuilder builder = null, Action<IWebHostBuilder> webHostconfigure = null,
            Action<IAppConfiguration> configure = null,
            Action<AppBuildOptions> buildOptions = null,
            string[] args = null)
            where T : class
        {
            webHost = host ?? webHost;
			var options = AppBuildOptions.Current;
			options.AppInfo?.Validate();
            bool configured = false;
            if (webHost == null)
            {
                buildOptions?.Invoke(options);
                var useStartUp = typeof(T) != typeof(string);
                webHostBuilder = builder ?? GetHostBuilder<T>(args);// WebHost.CreateDefaultBuilder<T>(args);
				/// Scope validations
				/// 
				webHostBuilder.UseDefaultServiceProvider(c => c.ValidateScopes = false);
                webHostconfigure?.Invoke(webHostBuilder);
                if (!string.IsNullOrWhiteSpace(options.NLogFileName) && File.Exists(options.NLogFileName))
                {
                    NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
                    webHostBuilder.UseNLog();
                }
                AppStartup_Deprecated.Configurator = x =>
                {
                    configured = true;
                    configure?.Invoke(x);
                };
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                webHostBuilder.UseContentRoot(pathToContentRoot);

                webHostBuilder.UseUrls(options.AppInfo.Validate().Urls);

                webHost = webHostBuilder.Build();
                //AppContext.Initialize(webHost.Services);
                //Initialized = true;
                //Context.AppServices.GetServices<Action<ModuleInitializationContext>>()
                //    .ToList().ForEach(x =>
                //    {
                //        try
                //        {
                //            x.Invoke(new ModuleInitializationContext
                //            {
                //                Context = AppHost.Context
                //            });
                //        }
                //        catch { }
                //    });
            }
            AppContext_Deprecated.Initialize(webHost.Services);
            configure?.Invoke(Configuration);
            Initialized = true;
            try
            {
                options.AppInfo.Update();
                //options.Save();
                //LibSettings.Current.Save();
            }
            catch { }

            return webHost;
        }
        /// <summary>
        /// Initializes host based on default settings.
        /// Use this method to initialize application (for instance the 
        /// service provider) without actually
        /// running the host for instance initializing the service provider.
        /// Use GetHost if you want to override initializations and settings.
        /// Use Run if you want to actually run the host.
        /// </summary>
        public static void Initialize(bool reset = false,
            Action<IAppConfiguration> configure = null, Action<AppBuildOptions> buildOptions = null)
        {
            Initialize<AppStartup_Deprecated>(
                reset: reset, configure: configure, buildOptions: buildOptions);

        }
        /// <summary>
        /// Initializes (without running) the host using startup class T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Initialize<T>(bool reset = false,
            Action<IAppConfiguration> configure = null, Action<AppBuildOptions> buildOptions = null) where T : class
        {
            if (reset)
                Reset();
            InitializeEx<T>(buildOptions: buildOptions, configure: configure);
            //configure?.Invoke(Context.Configuration);
        }
        public static void Reset()
        {
            webHost = null;
            webHostBuilder = null;
            AppContext_Deprecated.Reset();
            Initialized = false;
        }

        /// <summary>
        /// Initilaize, if necessary, and run the host with default settings.
        /// Use GetHost() and then run it if you want to override initialization.
        /// </summary>
        public static void Run(
            Action<IAppConfiguration> configure = null,
            Action<AppBuildOptions> buildOptions = null,
            Action onStarted = null,
            Action onStopping = null,
            Action onStopped = null
            )
        {
            Run<AppStartup_Deprecated>(configure: configure, buildOptions: buildOptions, onStarted: onStarted, onStopping: onStopping, onStopped: onStopped);
        }
        /// <summary>
        /// Initilaize, if necessary, and run the host with default settings.
        /// Use GetHost() and then run it if you want to override initialization.
        /// </summary>
        public static void Run<T>(
            Action<IAppConfiguration> configure = null,
            Action<AppBuildOptions> buildOptions = null,
            Action onStarted = null,
            Action onStopping = null,
            Action onStopped = null) where T : class
        {
            var host = InitializeEx<T>(buildOptions: buildOptions, configure: configure);
            var lifeTime = host.Services.GetService<Microsoft.AspNetCore.Hosting.IApplicationLifetime>();
            if (onStarted != null && lifeTime != null)
                lifeTime.ApplicationStarted.Register(onStarted);
            if (onStopping != null && lifeTime != null)
                lifeTime.ApplicationStopping.Register(onStopping);
            if (onStopped != null && lifeTime != null)
                lifeTime.ApplicationStopped.Register(onStopped);
            if (lifeTime!=null)
            {
                lifeTime.ApplicationStopping.Register(() =>
                {
                    Context.Configuration.Save();
                });
            }

            //configure?.Invoke(Context.Configuration);
            host.Run();
        }

        public static void AbstractRun(
            Action<IAppConfiguration> configure = null,
            Action<AppBuildOptions> buildOptions = null,
            Action onStarted = null,
            Action onStopping = null,
            Action onStopped = null)
        {
            AbstractRun<AppStartup_Deprecated>(configure: configure, buildOptions: buildOptions, onStarted: onStarted, onStopping: onStopping, onStopped: onStopped);
        }
        public static void AbstractRun<T>(
            Action<IAppConfiguration> configure = null,
            Action<AppBuildOptions> buildOptions = null,
            Action onStarted = null,
            Action onStopping = null,
            Action onStopped = null) where T : class
        {
            var host = AbstractInitialize<T>(buildOptions: buildOptions, configure: configure);
            var lifeTime = host.Services.GetService<Microsoft.AspNetCore.Hosting.IApplicationLifetime>();
            if (onStarted != null && lifeTime != null)
                lifeTime.ApplicationStarted.Register(onStarted);
            if (onStopping != null && lifeTime != null)
                lifeTime.ApplicationStopping.Register(onStopping);
            if (onStopped != null && lifeTime != null)
                lifeTime.ApplicationStopped.Register(onStopped);
            host.Run();
            //host.StartAsync(default(CancellationToken));
            //if (onStarted != null)
            //    onStarted();
            //host.WaitForShutdown();
            //host.WaitForShutdownAsync
        }

        private static ILoggerFactory LoggerFactory
        {
            get
            {
                if (AppContext_Deprecated.Initialized)
                    return AppContext_Deprecated.Current.AppServices.GetService<ILoggerFactory>();
                return null;
            }
        }
        public static GN.ILogger_Deprecated GetLogger<T>()
        {
            return LoggerFacrory.CreateLogger<T>(LoggerFactory);
        }
        public static ILogger_Deprecated GetLogger(Type type)
        {
            return LoggerFacrory.CreateLogger(LoggerFactory, type);
        }
        public static T GetService<T>()
        {
            return Services.GetService<T>();
        }
        public static IEnumerable<T> GetServices<T>()
        {
            return Services.GetServices<T>();
        }
        public static IAppContext_Deprecated Context { get { return AppContext_Deprecated.Current; } }

        public static IAppConfiguration Configuration =>
            AppContext_Deprecated.Initialized
            ? Context?.Configuration
            : AppStartup_Deprecated.AppConfiguration;

        public static IAppServices Services
        {
            get
            {
                return Context.AppServices;
            }
        }
        public static IAppUtils Utils { get { return Context.Utils; } }
        public static IAppModules Modules => AppModules.Instance;
        public static IAppObjectFactory Factory => Services.Factory;
        public static IAppMapper Mapper => AppMapper.Instance;
        //public static IMessageBus_Deprecated_2 MessageBus => Context.GetService<IMessageBus_Deprecated_2>();
        public static IAppDataServices DataContext => Services.GetService<IAppDataServices>();
    }

}
