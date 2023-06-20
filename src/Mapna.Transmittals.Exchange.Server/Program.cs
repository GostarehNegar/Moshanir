using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GN;
using GN.Library;
using GN.Library.Xrm;
using GN.Library.Messaging;
using Microsoft.AspNetCore.Builder;
using GN.Library.Win32;
using GN.Library.Win32.Hosting;
using NLog.Web;
using Microsoft.OpenApi.Models;
using GN.Library.SharePoint;

namespace Mapna.Transmittals.Exchange.Server
{
    public class Program
    {
        public static bool NetCore = true;
        public static void Main(string[] args)
        {
            try
            {
                //                    GN.Library.CommandLines.Internals.ConsoleApplicationHelper.Main(args)
                //                        .ConfigureAwait(false).GetAwaiter().GetResult();

#if (NET461_OR_GREATER)
                NetCore = false;
                CreateWindowsService(args).Run();
#else
                CreateHostBuilder(args).Build().UseGNLib().Run();
#endif

            }
            catch (Exception err)
            {
                Console.WriteLine(
                    $"An error occured while trying to start Program:\r\n{err}");
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

#if (NET461_OR_GREATER)

        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            return AppHost.GetWebHostBuilder()
                .UseDefaultServiceProvider(s => s.ValidateScopes = false)
                .ConfigureAppConfiguration(c => ConfigureAppConfiguration(c, args))
                .ConfigureLogging(logging => ConfigureLogging(logging))
                .ConfigureServices((c, s) =>
                {
                    NetCore = false;
                    ConfigureServices(c.Configuration, s, args);
                    s.AddMvc();
                })
                .Configure(app =>
                {
                    ConfigureApp(app);
                    app.UseGNLib();
                    app.UseStaticFiles();
                    app.UseSwagger();
                    app.UseSwaggerUI(c => {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Exchange API V1");
                    });

                    app.UseMvcWithDefaultRoute();
                    //app.UseSignalREventHub();
                })
                .UseNLog()
                .UseUrlsEx();
        }
        public static IWindowsServiceHost CreateWindowsService(string[] args)
        {
            return WindowsServiceHost.CreateDefaultBuilder(args)
                .UseWebHostBuilder(CreateHostBuilder(args))
                .ConfigureWindowsService("Mehregan.SMS.Server", "Mehregan SMS Server", null)
                .Build();
        }


#endif
#if (NETCOREAPP3_0_OR_GREATER)
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseDefaultServiceProvider(s => s.ValidateScopes = false)
                .ConfigureAppConfiguration(c => ConfigureAppConfiguration(c, args))
                .ConfigureLogging(logging => ConfigureLogging(logging))
                .ConfigureWebHostDefaults(cfg =>
                {
                    cfg.UseUrlsEx();
                    cfg.UseNLog();
                    cfg.Configure(app =>
                    {
                        ConfigureApp(app);
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                        app.UseSignalREventHub();
                    });
                })
                .ConfigureServices((c, s) =>
                {
                    NetCore = true;
                    ConfigureServices(c.Configuration, s, args);
                    s.AddControllers();
                });
#endif

        public static void ConfigureServices(IConfiguration configuration, IServiceCollection s, string[] args)
        {
            ConfigureNLog(args, configuration);
            AppInfo.Current.Name = configuration["name"] ?? Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            s.AddGNLib(configuration, cfg =>
            {
                cfg.SkipRedis();
            });
            //s.AddLibraryApi();
            s.AddMessagingServices(configuration, cfg => { cfg.Name = AppInfo.Current.Name; });
            s.AddSharePointServices(configuration, cfg => { });
            s.AddTransmittalsExchange(configuration, cfg => { });
            s.AddSwaggerGen(opt => {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Exchange API", Version = "v1" });
            });
            //s.AddSignalRTransport(configuration);

            //s.AddSignalRHub(configuration, cfg => { });
            //s.AddXrmServices(configuration, cfg =>
            //{
            //    //cfg.AddXrmMessageBus = false;
            //    cfg.ConnectionOptions = NetCore ? ConnectionOptions.WebAPI : ConnectionOptions.OrganizationService;
            //});
        }

        public static void ConfigureAppConfiguration(IConfigurationBuilder c, string[] args)
        {
            var configFile = "appsettings.json";
            c.AddJsonFile("appsettings.json");
            args = args ?? new string[] { };
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].ToLowerInvariant() == "-c" || args[i].ToLowerInvariant() == "--config" && i < args.Length - 1)
                {
                    configFile = args[i + 1];
                }
            }
            if (!File.Exists(configFile))
            {
                throw new Exception($"Configuration File Not Found:{configFile}");
            }
            Console.WriteLine($"Using Configuration: '{configFile}'");
            c.AddJsonFile(configFile);
            var switchMappings = new Dictionary<string, string>()
                 {
                     { "-n", "name" },
                     { "--name", "name" },
                     { "-c", "configfile" },
                     { "--config", "configfile" },
                 };
            c.AddCommandLine(args, switchMappings);
        }

        public static void ConfigureApp(IApplicationBuilder app)
        {

        }
        public static void ConfigureLogging(ILoggingBuilder logging)

        {
            logging.ClearProviders();
        }
        public static NLog.LogFactory ConfigureNLog(string[] args, IConfiguration configuration)
        {
            NLog.LogFactory result = null;
            var ffname = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            var name = configuration["name"] ?? configuration["applicationName"];
            name = string.IsNullOrWhiteSpace(name)
                ? Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0])
                : name;
            var folder = "logs";
            var default_layout = "${longdate}|${uppercase:${level}}|${logger}::: ${message} ${exception:format=tostring}";
            string getFileName(string n)
            {
                return $"{folder}\\{name} {n} [${{shortdate}}].log";
            }
            if (1 == 1 || !File.Exists("nlog.config"))
            {
                var config = new NLog.Config.LoggingConfiguration();
                config.AddTarget(new NLog.Targets.ColoredConsoleTarget
                {
                    Name = "console",
                    Layout = "${logger} (${level:uppercase=true}):::  ${message}"
                });
                config.AddTarget(new NLog.Targets.FileTarget
                {
                    Name = "trace",
                    FileName = getFileName("Trace"),
                    Layout = default_layout
                });
                config.AddTarget(new NLog.Targets.FileTarget
                {
                    Name = "info",
                    FileName = getFileName(""),
                    Layout = default_layout
                });
                config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, "trace");
                config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, "info");
                config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, "console");
                result = NLog.Web.NLogBuilder.ConfigureNLog(config);
                Console.WriteLine($"NLog Configured. FileName:'{getFileName("")}'");
                return result;
            }
            else
            {
                result = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config");
                //result.Configuration.Variables.Add("TTT", new NLog.Layouts.SimpleLayout { Text = "lll" });

                return result;
            }

        }

    }
}
