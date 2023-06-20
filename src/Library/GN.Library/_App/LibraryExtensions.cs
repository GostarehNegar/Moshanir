using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GN.Library.Xrm;
using Microsoft.Extensions.Logging;
using GN.Library.Serialization;
using GN.Library.WebCommands;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using GN.Library.TaskScheduling;
using GN.Library.Configurations;
using GN.Library;

using Microsoft.Extensions.Hosting;
using GN.Library.ServiceStatus;
using GN.Library.CommandLines_deprecated;
using GN.Library.Data;
using GN.Library.Helpers;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using GN.Library.HostedServices;
using GN.Library.Data.Internal;
using GN.Library.Data.Deprecated;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.IO;
using Microsoft.AspNetCore.Hosting;

using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using GN.Library.Services;
using Microsoft.AspNetCore;
using GN.Library.Messaging;
using System.Runtime.CompilerServices;
using GN.Library.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.RegularExpressions;
using GN.Library.Messaging.Deprecated;
using GN.Library.Data.Lite;
using GN.Library.Identity;
using GN.Library.Shared.Internals;
using GN.Library.ServiceDiscovery;
using GN.Library.Messaging.Queues;

namespace GN
{
    public static partial class Extensions
    {
        private class Grabber : IHostedService
        {
            public Grabber(IServiceProvider provider)
            {
                AppHost.Initialize(serviceProvider: provider);

            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
        internal static IFeatureCollection ServerFeatures;
        internal static void AddLibraryServices(this IServiceCollection services)
        {
            services.AddLibraryCoreServices();
            //services?.AddDataServices();
        }

        //public static IWebHostBuilder GetWebHostBuilder<T>(this IAppHostBuilder builder, string[] args = null, Action<AppInfo> configure = null) where T : class
        //{
        //    configure?.Invoke(AppInfo.Current);
        //    AppInfo.Current.Validate();

        //    var result = WebHost.CreateDefaultBuilder<T>(args);
        //    result.UseDefaultServiceProvider(s => s.ValidateScopes = false);
        //    return result.UseUrlsEx();
        //}
        public static ICurrentUser GetCurrentUser(this IAppContext This)
        {
            return This.Values.GetOrAddValue<ICurrentUser>(x => new CurrentUser());
        }
        public static ICurrentUser SetCurrentUser(this IAppContext This, ICurrentUser value)
        {
            return This.Values.AddOrUpdate<ICurrentUser>(_ => { return value; });
        }
        public static Microsoft.AspNetCore.Hosting.IWebHost UseGNLib(this Microsoft.AspNetCore.Hosting.IWebHost This)
        {

            AppContext.Initialzie(This.Services);
            //AppContext_Deprecated.Initialize(This.Services);
            ServerFeatures = This.ServerFeatures;
            return This;
        }
        public static IHost UseGNLib(this IHost This)
        {

            AppContext.Initialzie(This.Services);
            //AppContext_Deprecated.Initialize(This.Services);
            //ServerFeatures = This.ServerFeatures;
            return This;
        }
        private static string fixurl(string url)
        {
            if (url != null && url.EndsWith("/"))
            {
                return url.Substring(0, url.Length - 1);
            }
            return url;
        }
        public static IWebHostBuilder UseUrlsEx(this IWebHostBuilder builder, string urls = null)
        {
            urls = string.IsNullOrWhiteSpace(urls) ? AppInfo.Current.Urls : urls;
            urls = AppHost.Utils.GetAppUris(urls).Select(x => fixurl(x.AbsoluteUri))
                .Aggregate((current, next) => current + ";" + next);
            AppInfo.Current.Urls = urls;
            builder.UseUrls(AppInfo.Current.Urls);
            return builder;
        }
        public static Microsoft.Extensions.Hosting.IHost UseGnLib(this Microsoft.Extensions.Hosting.IHost host)
        {
            AppHost.Initialize(host);
            AppContext.Initialzie(host.Services);

            return host;
        }
        public static IServiceCollection AddGNLib(this IServiceCollection services, IConfiguration configuration, Action<LibOptions> configure, bool addMVC = true)
        {
            var options = LibOptions.Current;
            AppHost.Initialize(configuration: configuration);
            AppInfo.Current.Name = configuration["name"] ?? Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            configuration?.GetSection("Lib")?.Bind(options);
            configure?.Invoke(options);
            options = options.Validate();
            services.AddSingleton<LibOptions>(options);
            services.AddSingleton<IOptions<LibOptions>>(new OptionsWrapper<LibOptions>(options));
            if (!services.HasService<IAppContext>())
            {
                services.AddSingleton<IServiceCollection>(services);
                services.AddTransient<IAppContext>(s =>
                {
                    GN.AppContext.Initialzie(s);
                    return GN.AppContext.Current;
                });
                services.AddMemoryCache();
                services.AddSingleton<ServiceDiscoveryEx>();
                services.AddHostedService<ApplicationLifetimeManager>();
                services.AddSingleton<IServiceDiscoveryEx>(s => s.GetServiceEx<ServiceDiscoveryEx>());
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                //services.AddTransient<ILogger_Deprecated, AbstractLogger>();
                //services.AddTransient(typeof(IAbstractLogger<>), typeof(AbstractLogger<>));
                services.AddTransient(typeof(IDocumentRepository_Deprecated<>), typeof(DocumentRepository_Deprecated<>));
                services.AddTransient(typeof(IDocumentRepository<>), typeof(DocumentRepository<>));
                services.AddTransient(typeof(IDocumentRepository<,>), typeof(DocumentRepository<,>));
                services.AddTransient<IJsonSerializer, JsonSerializerEx>();
                services.AddTransient<IWebCommandFactory, WebCommandFactory>();
                services.AddTransient<IWebCommand, PingCommand>();
                services.AddTransient<IWebCommand, CommandLineWebCommand>();
                services.AddTransient<IAppUtils, AppUtils>();
                services.AddSingleton<IAppServerExplorer, AppServerExplorer>();
                services.AddTransient<CommandLine, TestCommand>();
                services.AddTransient<CommandLine, ServerCommand>();
                services.AddTransient<IMobilePhoneHelper, MobilePhoneHelper>();
                services.AddTransient<IAppMapper>(s => AppMapper.Instance);
                services.AddSingleton<IFileStorage, FileStorage>();
                services.AddTransient<ICurrentUser>(p =>
                {
                    return AppContext.Current.Values.GetOrAddValue<ICurrentUser>(x => new CurrentUser());
                }
                );

                if (options.HealthCheck.Enabled)
                {
                    services.AddTransient<ServiceStatusTask>();
                    services.AddTransient<IScheduledTask, ServiceStatusTask>();
                }
                services.AddTransient<IAppServices>(x => { return AppContext.Current.AppServices; });
                //services.AddTransient<IAppContext>(x => { return AppContext.Current; });
                //services.AddTransient<IAppConfiguration>(x => { return AppHost.Context.AppConfigurations; });
                //services.AddTransient<IUserDataContext, UserDataContext>();
                //services.AddTransient<IPublicDataContext, PublicDataContext>();
                //services.AddTransient<ILocalDataContext, LocalDataContext>();

                services.AddTransient<LocalDbConfig>();
                services.AddTransient<ILocalDocumentStore, LocalDb>();
                services.AddTransient(typeof(ILocalDocumentStoreRepository<,>), typeof(LocalDbRepository<,>));

                services.AddTransient<PublicDbConfig>();
                services.AddTransient<IPublicDocumentStore, PublicDb>();
                services.AddTransient(typeof(IPublicDbRepository<,>), typeof(PublicDbRepository<,>));

                services.AddTransient<UserDbConfig>();
                services.AddTransient<IUserDocumentStore, UserDb>();
                services.AddTransient(typeof(IUserDocumentStoreRepository<,>), typeof(UserDbRepository<,>));
                services.AddTransient(typeof(IDynamicEntityRepository<>), typeof(DynamicEntityLiteDBRepository<>));
                //services.AddTransient<IGlobalDataContext, GlobalDataContext>();
                services.AddScoped<IAppDataServices, AppDataContext>();
                services.AddSingleton<IHostedService, HostingService>();
                services.AddScheduler((sender, args) =>
                {
                    Console.Write(args.Exception.Message);
                    args.SetObserved();
                });
                services.AddMessagingServices(ConfigureBus);
                services.AddMemoryCache();
                services.AddDistributedMemoryCache();
                if (!options.skip_redis)
                {
                    GN.Library.Redis.RedisExtensions.AddRedis(services, configuration, options.redis_config);
                }
                services.AddSingleton<TokenOptions>();
                services.AddSingleton<ITokenService, TokenService>();
                services.AddSingleton<ILibraryConventions, LibraryConventions>();
                services.AddSingleton<UserServices>();
                services.AddSingleton<IUserServices>(sp => sp.GetServiceEx<UserServices>());
                if (options.UserService.Enabled)
                {
                    services.AddSingleton<LocalUserRepository>();
                    services.AddTransient<ILocalUserRepository>(sp => sp.GetService<LocalUserRepository>());
                    services.AddSingleton<LocalUserServices>();
                    services.AddHostedService(sp => sp.GetServiceEx<LocalUserServices>());
                }

                if (configuration.GetMessaginQueueOptions().Enabled)
                {
                    services.AddMessagingQueue(configuration.GetMessaginQueueOptions());
                }
            }
            return services;
        }

        internal static void ConfigureBus(GN.Library.Messaging.Internals.IMessageBusConfigurator config)
        {
            config.Register(subs =>
            {
                subs.UseTopic(typeof(HealthCommand));
                subs.UseHandler<HealthCommand>(ctx =>
                {
                    return ActivatorUtilities.CreateInstance<HealthCommandHandler>(subs.ServiceProvider).Handle(ctx);
                });
            });
        }

        public static IAppMapper GetMapper(this IServiceCollection services)
        {
            return AppMapper.Instance;
        }
        internal static void AddLibraryCoreServices(this IServiceCollection services)
        {
            var options = AppBuildOptions.Current;
            if (!services.HasService<IWebCommand>())
            {
                //services.AddTransient<ILogger_Deprecated, AbstractLogger>();
                //services.AddTransient(typeof(IAbstractLogger<>), typeof(AbstractLogger<>));
                services.AddTransient(typeof(IDocumentRepository_Deprecated<>), typeof(DocumentRepository_Deprecated<>));
                services.AddTransient(typeof(IDocumentRepository<>), typeof(DocumentRepository<>));
                services.AddTransient<IJsonSerializer, JsonSerializerEx>();
                services.AddTransient<IWebCommandFactory, WebCommandFactory>();
                services.AddTransient<IWebCommand, PingCommand>();
                services.AddTransient<IWebCommand, CommandLineWebCommand>();
                services.AddTransient<IAppUtils, AppUtils>();
                services.AddSingleton<IAppServerExplorer, AppServerExplorer>();
                services.AddTransient<CommandLine, TestCommand>();
                services.AddTransient<CommandLine, ServerCommand>();
                services.AddTransient<IMobilePhoneHelper, MobilePhoneHelper>();
                services.AddTransient<IAppMapper>(s => AppMapper.Instance);
                //services.AddTransient<ICurrentUser>(p => AppContext.Current.GetOrAddValue<ICurrentUser>(x => new CurrentUser()));
                services.AddTransient<IScheduledTask, ServiceStatusTask>();
                services.AddTransient<IAppServices>(x => { return AppContext.Current.AppServices; });
                services.AddTransient<IAppContext>(x => { return AppContext.Current; });
                //services.AddTransient<IAppConfiguration>(x => { return AppContext.Current.AppConfigurations; });
                //services.AddTransient<IUserDataContext, UserDataContext>();
                //services.AddTransient<IPublicDataContext, PublicDataContext>();
                //services.AddTransient<ILocalDataContext, LocalDataContext>();
                //services.AddTransient<IGlobalDataContext, GlobalDataContext>();
                services.AddScoped<IAppDataServices, AppDataContext>();
                services.AddSingleton<IHostedService, HostingService>();
                services.AddScheduler((sender, args) =>
                {
                    Console.Write(args.Exception.Message);
                    args.SetObserved();

                });
            }
        }

        //public static void UseGNLibrary(this IServiceProvider This)
        //{
        //    AppHost.Initialize(null, null, This);
        //    GN.AppContext.Initialzie(This);
        //}
        public static void UseGNLib(this IApplicationBuilder This, bool AddMVC = true)
        {
            AppHost.Initialize(serviceProvider: This.ApplicationServices);
            ServerFeatures = This.ServerFeatures;
            GN.AppContext.Initialzie(This.ApplicationServices);
        }
        public static AppBuildOptions GetBuildOptions(this IConfiguration This)
        {
            return AppBuildOptions.Current;
        }
        //public static ILogger_Deprecated GetLogger(this Type This)
        //{
        //    return AppHost.GetLogger(This);
        //}
        public static ILogger GetLoggerEx(this Type type)
        {
            return AppHost.Services.GetService<ILoggerFactory>().CreateLogger(type); 
        }
        private static T GetService<T>()
        {
            return AppHost.GetService<T>();
        }

        public static string Serialize(this IAppUtils This, object Object)
        {

            return GetService<IJsonSerializer>().Serialize(Object);
        }
        public static bool TryDeserialize<T>(this IAppUtils This, string input, out T output)
        {

            try
            {
                output = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(input);
                return true;
            }
            catch { }
            output = default(T);
            return false;
        }
        public static bool TryNewtonsoftDeserialize(this IAppUtils This, string input, Type type, out object output)
        {
            try
            {
                output = Newtonsoft.Json.JsonConvert.DeserializeObject(input, type);
                return true;
            }
            catch { }
            output = null;
            return false;
        }
        public static string NewtonsoftSerialize(this IAppUtils This, object target)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(target);
        }
        public static T Deserialize<T>(this IAppUtils This, string data)
        {
            return GetService<IJsonSerializer>().Deserialize<T>(data);
        }
        public static string Serialize2(this IAppUtils This, object Object)
        {
            return GetService<IJsonSerializer>().Serialize2(Object);
        }

        public static string GetEntityLogicalName(this Type type)
        {
            throw new NotImplementedException();
        }
        public static string GetEntityLogicalName(this IAppUtils This, Type type)
        {
            throw new NotImplementedException();
        }

        public static bool HasService<T>(this IServiceCollection This)
        {
            return This.Any(x => x.ServiceType == typeof(T));
        }




        public static string Left(this string This, int length)
        {
            return This == null ? null :
                (This.Substring(0, (length > This.Length ? This.Length : length)));
        }
        public static string Right(this string This, int length)
        {
            return This == null
                ? null
                : This.Length > length
                ? This.Substring(This.Length - length, length)
                : This;
        }
        public static IMobilePhoneHelper ParseMobilePhone(this IAppUtils This, string mobilePhone)
        {
            return new MobilePhoneHelper().Parse(mobilePhone);
        }
        public static Task<T> TimeOutAfter<T>(this Task<T> task, int timeOut,
            CancellationToken cancellation = default(CancellationToken),
            bool throwIfTimeOut = false)
        {
            return UtilityHelpers.TimeoutAfter<T>(task, timeOut, cancellation, throwIfTimeOut);
        }
        public static Task TimeOutAfter(this Task task, int timeOut,
            CancellationToken cancellation = default(CancellationToken),
            bool throwIfTimeOut = false)
        {
            return UtilityHelpers.TimeoutAfter(task, timeOut, cancellation, throwIfTimeOut);
        }
        public static object GetValue(this IDictionary<string, object> This, Type type, string key)
        {
            object result = null;
            if (key == null)
            {
                return This.Values.Where(x => x != null && type.IsAssignableFrom(x.GetType())).FirstOrDefault();
            }
            else if (This.TryGetValue(key, out var _result))
            {
                result = _result;
            }
            return result;

        }
        public static string GetCurrentApplicationName(this IAppUtils This)
        {
            return Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
        }
        public static string GetCurrentApplicationDirectory(this IAppUtils This)
        {
            return Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        }

        public static string GetUrlsFromAppSettings()
        {
            string urls = "";
            var args = Environment.GetCommandLineArgs();
            var configFile = "appsettings.json";
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].ToLowerInvariant() == "-c" || args[i].ToLowerInvariant() == "--config" && i < args.Length - 1)
                {
                    configFile = args[i + 1];
                }
            }
            if (File.Exists(configFile))
            {
                try
                {
                    urls = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(
                        File.ReadAllText(configFile))
                        .Value<string>("urls");
                    if (string.IsNullOrWhiteSpace(urls))
                    {
                        urls = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(
                            File.ReadAllText(configFile))
                            .Value<string>("Urls");
                    }
                }
                catch { }
            }
            return urls ?? "";
        }
        public static string GetUrlsFromAppSettings(this IAppUtils This)
        {
            return GetUrlsFromAppSettings(null);
        }
        public static string[] GetLocalIPs(this IAppUtils This)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).Select(x => x.ToString()).ToList().ToArray();
        }

        public static Uri GetAppUri(this IAppUtils This, bool ExcludeLocalHost = false, int? port = null)
        {
            var urls = This.GetAppUris(port: port);
            var result = urls.FirstOrDefault(x => !x.IsLoopback) ?? (ExcludeLocalHost ? null : urls.FirstOrDefault());
            return result;

        }
        public static string GetAppUrl(this IAppUtils This, bool ExcludeLocalHost = false, int? port = null)
        {
            return This.GetAppUri(ExcludeLocalHost, port: port)?.AbsoluteUri;

        }
        /// <summary>
        /// Returns a set of candidate Uris for the application by:
        /// 1. Inspection 'Addresses' property of the IServerAddressesFeature.
        /// 2. Inspection'--urls' arguements.
        /// 3. Inspecting 'urls' property in appsettings.
        /// 4. Inspection AppInfo.Urls
        /// Will default to 'http:*:port' if nonoe of the above settings is present.
        /// 
        /// </summary>
        /// <remarks>
        /// The code works in two different modes, if the application is  already running
        /// it will uses the 'Addresses' propery of the IServerAddressesFeature to capture
        /// the actual urls that are currently in place. Otherwise it will do it's best
        /// to comeup with the candiate urls based on commanf line arguments, appsettings ...
        /// </remarks>
        /// <param name="This"></param>
        /// <param name="urls"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static Uri[] GetAppUris(this IAppUtils This, string urls = null, int? port = null)
        {
            var result = new List<string>();
            port = port ?? new Random().Next(2300, 2400);
            void addUrl(string url)
            {
                if (!string.IsNullOrEmpty(url))
                {
                    url = url
                        .ToLowerInvariant()
                        .Replace("http://*", "http://[::]")
                        .Replace("http://[*]", "http://[::]");
                    if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    {
                        result.Add(url);
                    }
                }
            }
            try
            {
                var addresses = ServerFeatures?.Get<IServerAddressesFeature>() != null
                    ? ServerFeatures.Get<IServerAddressesFeature>().Addresses.ToArray()
                    : new string[] { };

                foreach (var item in addresses)
                {
                    foreach (var url in item.Split(',', ';'))
                    {
                        addUrl(url);
                    }
                }
                if (result.Count < 1)
                {
                    var args = Environment.GetCommandLineArgs();
                    for (var i = 0; i < args.Length; i++)
                    {
                        if (args[i] != null && args[i].ToLowerInvariant() == "--urls" && i < args.Length - 1)
                        {
                            foreach (var url in args[i + 1].Split(',', ';'))
                            {
                                if (!string.IsNullOrEmpty(url))
                                    addUrl(url);
                            }
                        }
                    }
                }
                if (result.Count < 1)
                {
                    /// Get Urls from appsetiings.json
                    /// 
                    foreach (var url in GetUrlsFromAppSettings().Split(';', ','))
                    {
                        addUrl(url);
                    }
                }
                if (result.Count < 1 && !string.IsNullOrWhiteSpace(urls))
                {
                    foreach (var url in urls.Split(';', ','))
                    {
                        addUrl(url);
                    }
                }
            }
            catch (Exception) { }
            if (result.Count < 1)
            {
                addUrl($"http://*:{port}");
            }
            var _result = new List<string>();
            foreach (var item in result)
            {
                if (1 == 0 && item.Contains("[::]"))
                {
                    foreach (var ip in GetLocalIPs(null))
                    {
                        _result.Add(item.Replace("[::]", ip));
                    }
                    _result.Add(item.Replace("[::]", "localhost"));
                }
                else
                {
                    _result.Add(item);
                }
            }
            return _result.DistinctBy(x => x).ToArray()
                .Where(x => Uri.IsWellFormedUriString(x, UriKind.Absolute))
                .Select(x => new Uri(x))
                .ToArray();

        }

        public static string[] GetAppUrls(this IAppUtils This)
        {
            return This.GetAppUris().Select(x => x.AbsoluteUri).ToArray();
        }
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        public static string GetMicroServiceUniqueInstanceId(this IAppUtils utils)
        {
            var info = GN.AppHost.AppInfo;
            return Environment.MachineName;
        }

        public static string GetCurrentServiceNameBasedOnProcessName(this IAppUtils This)
        {
            return This.GetCurrentApplicationName();
        }

        public static T GetServiceEx<T>(this IServiceProvider provider)
        {
            return (T)provider.GetService(typeof(T));
        }
        public static string GetForwardedHostAddress(this HttpRequest request)
        {
            string result = null;
            var host = request?.Headers[XForwardedHost].FirstOrDefault();
            var proto = request?.Headers[XForwardedProto].FirstOrDefault();
            var path = request?.Headers[XForwardedPathBase].FirstOrDefault() ?? "//";
            if (path != null)
            {

                path = path.Split('/').Length > 2 ? path.Split('/')[2] : "";

            }
            if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(proto))
            {
                var url = string.Format("{0}://{1}/{2}", proto, host, path);
                result = Uri.IsWellFormedUriString(url, UriKind.Absolute)
                    ? url
                    : null;
            }
            return result;
        }
        internal const string XForwardedFor = "X-Forwarded-For";
        internal const string XForwardedHost = "X-Forwarded-Host";
        internal const string XForwardedProto = "X-Forwarded-Proto";
        internal const string XForwardedPathBase = "X-Forwarded-PathBase";

        public static T Convert<T>(object source, bool useSerialization = true)
        {
            return TryConvert<T>(source, out var _d, useSerialization) ? _d : default(T);
        }
        public static bool TryConvert<T>(object source, out T dest, bool useSerialization = true)
        {
            dest = default(T);
            var result = TryConvert(source, typeof(T), out var _dest, useSerialization);
            if (result)
                dest = (T)_dest;
            return result;
        }

        public static T Convert<T>(this IAppUtils utiles, object source, bool userSerialization = true)
        {
            return Convert<T>(source, userSerialization);
        }

        public static bool TryConvert(object source, Type type, out object result, bool useSerialization = true)
        {
            return GN.Library.Helpers.Converter.TryConvert(source, type, out result, useSerialization);
            result = null;
            if (source == null && type.IsNullable())
                return true;
            if (source == null)
                return false;
            if (type == null)
                return false;
            var sourceType = source.GetType();
            if (type.IsAssignableFrom(source.GetType()))
            {
                result = source;
                return true;
            }
            if (type == typeof(string))
            {
                if (!sourceType.IsValueType && sourceType != typeof(string) && useSerialization)
                {
                    try
                    {
                        result = Newtonsoft.Json.JsonConvert.SerializeObject(source);
                        return true;
                    }
                    catch { }
                }
                result = source.ToString();
                return true;
            }
            if ((type == typeof(int) || type == typeof(int?)) && int.TryParse(source.ToString(), out var tmp))
            {
                result = tmp;
                return true;
            }
            if ((type == typeof(Guid?) || type == typeof(Guid)) && Guid.TryParse(source.ToString(), out var _tmp))
            {
                result = _tmp;
                return true;
            }
            if ((type == typeof(DateTime?) || type == typeof(DateTime)) && DateTime.TryParse(source.ToString(), out var __tmp))
            {
                result = __tmp;
                return true;
            }
            /// Try using Newtonsoft
            /// 
            if (useSerialization)
            {
                var _result = source.ToString();
                if ((type == typeof(Guid) || type == typeof(Guid?)) && !_result.StartsWith("\""))
                    _result = "\"" + _result.ToString() + "\"";
                try
                {
                    result = Newtonsoft.Json.JsonConvert.DeserializeObject(_result, type);
                    return true;
                }
                catch { }
            }
            return false;
        }
        internal static bool InternalTryConvert(this IAppUtils utils, object source, Type type, out object result)
        {
            result = null;
            if (source == null && type.IsNullable())
                return true;
            if (source == null)
                return false;
            if (type == null)
                return false;
            if (type.IsAssignableFrom(source.GetType()))
            {
                result = source;
                return true;
            }
            if (type == typeof(string))
            {
                result = source.ToString();
                return true;
            }
            if ((type == typeof(int) || type == typeof(int?)) && int.TryParse(source.ToString(), out var tmp))
            {
                result = tmp;
                return true;
            }
            if ((type == typeof(Guid?) || type == typeof(Guid)) && Guid.TryParse(source.ToString(), out var _tmp))
            {
                result = _tmp;
                return true;
            }
            if ((type == typeof(DateTime?) || type == typeof(DateTime)) && DateTime.TryParse(source.ToString(), out var __tmp))
            {
                result = __tmp;
                return true;
            }
            return false;
        }


        public static HealthCheckResult Healthy(this HealthCheckContext context, string description)
        {
            var values = new Dictionary<string, object>()
            {

            };
            values.Add("_", new StringWriter());
            return HealthCheckResult.Healthy(description, values);
        }
        public static HealthCheckResult Unhealthy(this HealthCheckContext context, string description, Exception exception = null)
        {
            var values = new Dictionary<string, object>()
            {

            };
            values.Add("_", new StringWriter());
            return HealthCheckResult.Unhealthy(description, exception, values);
        }
        public static HealthCheckResult WriteLine(this HealthCheckResult health, string message, params object[] args)
        {
            if (health.Data.TryGetValue("_", out var w) && w as StringWriter != null)
            {
                (w as StringWriter).WriteLine(message, args);
            }
            return health;
        }
        public static string GetReport(this HealthCheckResult health)
        {
            if (health.Data.TryGetValue("_", out var w) && w as StringWriter != null)
            {
                return (w as StringWriter).ToString();
            }
            return string.Empty;
        }
        public static IAuthorizationService GetAuthorizationServices(this IAppServices services)
        {
            return services.GetService<IAuthorizationService>();
        }
        public static IMessageBus_Deprecated MessageBus_Deprecated(this IAppServices services)
        {
            return services.GetService<IMessageBus_Deprecated>();
        }
        public static bool IsNullable(this Type type)
        {
            return type == null || !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        public static bool WildCardMatch(string value, string pattern)
        {
            if (value == null || pattern == null)
                return false;
            var exp = "^" + Regex.Escape(pattern).Replace("\\?", ".").Replace("\\*", ".*") + "$";
            return Regex.IsMatch(value, exp);
        }

        public static IEnumerable<T> EnumerableOrEmpty<T>(this IEnumerable<T> This)
        {
            return This ?? Enumerable.Empty<T>();
        }
    }






}
