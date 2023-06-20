using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library
{
    public enum HostTypes
    {
        Generic,
        Web
    }
    public interface IAbstactHostBuilder
    {
        IAbstractHost Build();
        IWebHostBuilder WebHostBuilder { get; }
        IHostBuilder HostBuilder { get; }
        HostTypes HostType { get; }

    }
    public interface IAbstractHost : IHost
    {

    }
    public class AbstractHostBuilder<T> : IAbstactHostBuilder where T : class
    {
        public Type StartUpType { get; private set; }
        public HostTypes HostType { get; private set; }
        public IWebHostBuilder WebHostBuilder { get; private set; }
        public IHostBuilder HostBuilder { get; private set; }
        public AbstractHostBuilder(HostTypes type, string[] args)
        {
            StartUpType = typeof(T);
            this.HostType = type;
            this.WebHostBuilder = WebHost.CreateDefaultBuilder<T>(args);
            this.HostBuilder = new HostBuilder();
            AppStartup_Deprecated appStartup = null;
            this.HostBuilder
                .ConfigureLogging((x, y) =>
                {
                    y.AddConsole()
                    .AddDebug();
                    try
                    {
                        var f = x.HostingEnvironment;
                        var env = new Microsoft.AspNetCore.Hosting.Internal.HostingEnvironment
                        {
                            ApplicationName = x.HostingEnvironment?.ApplicationName,
                            EnvironmentName = x.HostingEnvironment?.EnvironmentName,
                            ContentRootPath = x.HostingEnvironment?.ContentRootPath,
                            ContentRootFileProvider = x.HostingEnvironment?.ContentRootFileProvider
                        };
                        appStartup = Activator.CreateInstance(StartUpType, new object[] { env }) as AppStartup_Deprecated;
                    }
                    catch { }
                })
                .ConfigureServices(s =>
                {
                    appStartup?.ConfigureServices(s);
                });
        }

        public IAbstractHost Build()
        {
            IWebHost webHost = this.HostType == HostTypes.Web ? this.WebHostBuilder.Build() : null;
            IHost host = this.HostType == HostTypes.Generic ? this.HostBuilder.Build() : null;
            return new AbstractHost(webHost, host);
        }
    }
    public class AbstractHost : IAbstractHost
    {
        public IWebHost WebHost { get; private set; }
        public IHost Host { get; private set; }
        public AbstractHost(IWebHost webHost, IHost host)
        {
            this.WebHost = webHost;
            this.Host = host;
        }

        public IServiceProvider Services
        {
            get
            {
                return this.WebHost == null
                    ? this.Host == null ? null : this.Host.Services
                    : this.WebHost.Services;
            }
        }

        public static IAbstactHostBuilder CreateDefaultBuilder<T>(HostTypes hosttype = HostTypes.Web, string[] args = null) where T : class
        {
            return new AbstractHostBuilder<T>(hosttype, args);
        }

        public static void Test()
        {
            var ggg = new HostBuilder();
            var fff = ggg.Build();


        }

        public void Dispose()
        {
            this.WebHost?.Dispose();
            this.Host?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {

            if (this.WebHost != null)
                return this.WebHost.StartAsync(cancellationToken);
            else
                return this.Host.StartAsync(cancellationToken);


        }

        public Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.WebHost != null)
                return this.WebHost.StopAsync(cancellationToken);
            else
                return this.Host.StopAsync(cancellationToken);
        }
    }

    static class Abs
    {
        public static IHostBuilder UseNLog(this IHostBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices(services =>
            {
                ConfigurationItemFactory.Default.RegisterItemsFromAssembly(
                    typeof(NLog.ILogger).GetTypeInfo().Assembly);

                LogManager.AddHiddenAssembly(typeof(Abs).GetTypeInfo().Assembly);

                services.AddSingleton(new LoggerFactory().AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                }));
            });

            return builder;
        }

    }
}
