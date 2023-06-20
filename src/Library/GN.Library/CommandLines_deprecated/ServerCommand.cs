using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.CommandLines_deprecated
{
    public class ServerCommand : CommandLine
    {
        protected override string Name => "WinService";
        public static ILogger logger = typeof(ServerCommand).GetLoggerEx();

        private CommandOption Install;
        private CommandOption UnInstall;
        public override void DoConfigure(CommandLineApplicationEx command)
        {
            this.Install = command.Option("-i|--install", "Installs this server as Windows Service", CommandOptionType.NoValue);
            this.UnInstall = command.Option("-u|--uninstall", "Uninstalls this server's Windows Service.", CommandOptionType.NoValue);
        }

        public string GetServiceName()
        {
            return AppBuildOptions.Current.AppInfo.Name;
        }
        public bool DoInstall(CommandLineApplicationEx command)
        {
            var result = false;
            var serviceName = GetServiceName();
            var displayName = AppHost.AppInfo.DisplayName;// AppHost_Deprectated.Configuration.Options.AppInfo.DisplayName;
            if (OpenSource.ServiceInstaller.ServiceIsInstalled(serviceName))
            {
                command.WriteLine($"Service Already Installed. Service Name:{serviceName}");
            }
            else
            {
                try
                {
                    var path = Process.GetCurrentProcess().MainModule.FileName;
                    OpenSource.ServiceInstaller.InstallAndStart(
                        serviceName: serviceName,
                        displayName: displayName,
                        fileName: path);
                    result = OpenSource.ServiceInstaller.ServiceIsInstalled(serviceName);
                    if (result)
                    {
                        command.WriteLine(
                            "Server Service Successfully Installed." +
                            $"Service:{serviceName}, Machine:{Environment.MachineName}");
                    }
                    else
                    {
                        throw new Exception(
                            $"Failed to Install Server Service on this Machine. MachineName:{Environment.MachineName}");
                    }
                }
                catch (Exception err)
                {
                    throw;
                }
            }
            return result;
        }

        public bool DoUninstall(CommandLineApplicationEx command)
        {
            var result = false;
            var serviceName = GetServiceName();
            OpenSource.ServiceInstaller.StopService(serviceName);
            OpenSource.ServiceInstaller.Uninstall(serviceName);
            result = OpenSource.ServiceInstaller.ServiceIsInstalled(serviceName);
            if (!result)
            {
                command.WriteLine("Service Successfully Uninstalled.");
            }
            else
            {
                throw new Exception(
                    $"Failed to uninstall service. Service:{serviceName}, MachineName:{Environment.MachineName}");
            }

            return result;
        }

        public async override Task<int> DoExecute(CommandLineApplicationEx command)
        {
            var result = await Task.FromResult(0).ConfigureAwait(false);
            if (this.Install.HasValue())
            {
                DoInstall(command);
            }
            else if (this.UnInstall.HasValue())
            {
                DoUninstall(command);
            }
            return result;
        }
    }
}
