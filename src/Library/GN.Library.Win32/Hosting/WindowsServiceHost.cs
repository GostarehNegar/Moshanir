using GN.Library.Win32.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Activation.Configuration;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Win32.Hosting
{
	public class WindowsServiceHost : ServiceBase, IWindowsServiceHost
	{
		public IServiceProvider Services => this.webHost?.Services ?? this.host?.Services;
		private IWebHost webHost;
		private IHost host;
		private WindowsServiceBuilder builder;

		internal WindowsServiceHost(WindowsServiceBuilder builder, IWebHost webHost, IHost host)
		{
			this.webHost = webHost;
			this.host = host;
			this.builder = builder;
		}

		protected override void OnStart(string[] args)
		{
			//base.OnStart(args);
			
			this.DoStartAsync(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult();
		}
		protected override void OnStop()
		{
			this.DoStopAsync(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public static WindowsServiceBuilder CreateDefaultBuilder(string[] args)
		{
			// By default current direcory is 'windows\system32' for 
			// windows services. We will fix it here.
			if (IsInWindowsService())
			{
				System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
			}
			return new WindowsServiceBuilder(args);
		}

		public async Task DoStartAsync(CancellationToken cancellationToken = default)
		{
			if (this.webHost != null)
				await this.webHost.StartAsync(cancellationToken);
			if (this.host != null)
				await this.host.StartAsync(cancellationToken);
		}

		public async Task DoStopAsync(CancellationToken cancellationToken = default)
		{
			if (this.webHost != null)
				await this.webHost.StopAsync(cancellationToken);
			if (this.host != null)
				await this.host.StopAsync(cancellationToken);
		}

		public static bool IsInWindowsService()
		{
			return !Environment.UserInteractive;
		}

		private bool? IsServiceInstalled()
		{
			bool? result = null;
			try
			{
				return GN.Library.Win32.Helpers.WindowsServiceInstaller.ServiceIsInstalled(this.builder.ServiceName);
			}
			catch
			{

			}
			return null;
		}
		private bool TryInstallService()
		{
			var result = false;
			try
			{
				result = WindowsServiceInstaller.ServiceIsInstalled(this.builder.ServiceName);
				if (!result)
				{
					var executable = Environment.GetCommandLineArgs()[0];
					if (!Path.IsPathRooted(executable))
					{
						executable = Path.GetFullPath(executable);
					}
					//Console.WriteLine(executable);
					WindowsServiceInstaller.Install(this.builder.ServiceName, this.builder.ServiceDisplayName, executable, false);
					result = WindowsServiceInstaller.ServiceIsInstalled(this.builder.ServiceName);
					if (!result)
					{
						throw new Exception($"Failed to install windows service: {this.builder.ServiceName}. " +
							"This often happens because of insufficient user priviliges.");
					}
					Console.WriteLine(
						$"Windows service '{this.builder.ServiceName}' successfully installed. ");
				}
				else
				{
					Console.WriteLine(
						$"Windows service '{this.builder.ServiceName}' is already installed. You may run it from Service Control Panel.");

				}
			}
			catch (Exception err)
			{
				Console.WriteLine($"An error occured while trying to install windows service. \r\n" +
					"This often occures becuase of 'Insuffucient User Priviliges'. Please consider running as administrator." +
					$"Err:{err.GetBaseException().Message}");
			}
			return result; ;
		}

		private bool TryUnInstallService()
		{
			var result = false;
			try
			{
				result = !WindowsServiceInstaller.ServiceIsInstalled(this.builder.ServiceName);
				if (!result)
				{
					if (!WindowsServiceInstaller.IsSCMAvailable(true))
					{

					}
					WindowsServiceInstaller.Uninstall(this.builder.ServiceName);
					result = !WindowsServiceInstaller.ServiceIsInstalled(this.builder.ServiceName);
					if (!result)
					{
						throw new Exception($"Failed to uninstall windows service: {this.builder.ServiceName}. " +
							"This often happens because of insufficient user priviliges.");
					}
					Console.WriteLine(
						$"Windows service '{this.builder.ServiceName}' successfully uninstalled. ");
				}
				else
				{
					Console.WriteLine(
						$"Windows service '{this.builder.ServiceName}' is already uninstalled.");
				}
			}

			catch (Exception err)
			{
				Console.WriteLine($"An error occured while trying to uninstall windows service. \r\n" +
					"This often occures becuase of 'Insuffucient User Priviliges'. Please consider running as administrator." +
					$"Err:{err.GetBaseException().Message}");
			}
			return result; ;
		}
		public async Task StartAsync(CancellationToken cancellationToken = default)
		{
			if (IsInWindowsService())
			{
				var ServicesToRun = new ServiceBase[] { this };
				ServiceBase.Run(ServicesToRun);
			}
			else
			{
				if (!WindowsServiceInstaller.IsSCMAvailable(false))
				{

				}
				var args = Environment.GetCommandLineArgs();
				if (args.Length > 1 && args[1].ToLowerInvariant() == "install")
				{

					Console.WriteLine("Trying to install windows service...");
					this.TryInstallService();
					Environment.Exit(0);

				}
				else if (args.Length > 1 && args[1].ToLowerInvariant() == "uninstall")
				{
					Console.WriteLine("Trying to uninstall windows service...");
					this.TryUnInstallService();
					Environment.Exit(0);

				}
				else
				{
					try
					{
						await this.DoStartAsync(cancellationToken);
					}
					catch (Exception err)
					{

					}

				}




			}
		}

		public Task StopAsync(CancellationToken cancellationToken = default)
		{
			return this.DoStopAsync();
		}

		public IWebHost GetHost()
		{
			return this.webHost;
		}
		public void Run()
		{
			this.RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}
		public async Task RunAsync(CancellationToken cancellationToken = default)
		{
			if (IsInWindowsService())
			{
				System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
				var ServicesToRun = new ServiceBase[] { this };
				ServiceBase.Run(ServicesToRun);
			}
			else
			{
				if (!WindowsServiceInstaller.IsSCMAvailable(false))
				{

				}
				var args = Environment.GetCommandLineArgs();
				if (args.Length > 1 && args[1].ToLowerInvariant() == "install")
				{

					Console.WriteLine("Trying to install windows service...");
					this.TryInstallService();
					Environment.Exit(0);

				}
				else if (args.Length > 1 && args[1].ToLowerInvariant() == "uninstall")
				{
					Console.WriteLine("Trying to uninstall windows service...");
					this.TryUnInstallService();
					Environment.Exit(0);

				}
				else
				{
					try
					{
						if (this.webHost != null)
							await this.webHost.RunAsync(cancellationToken);
						if (this.host != null)
							await this.host.RunAsync(cancellationToken);
					}
					catch (Exception err)
					{
						Console.WriteLine(err);
					}
				}
			}


		}

	}
}
