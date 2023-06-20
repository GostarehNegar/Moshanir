using GN.Library.Shared.ServiceDiscovery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace GN.Library.ServerManagement
{
    public class ProcessWrapper
    {
        private readonly ILogger logger;
        public Process Process { get; private set; }

        public string Name { get; }
        public ProcessStartInfo ProcessStartInfo { get; }
        public NodeData NodeData { get; }
        public ServiceInfo ServiceInfo { get; }


        public ProcessWrapper(string Name, ProcessStartInfo processStartInfo, ILogger logger)
        {
            this.Name = Name;
            ProcessStartInfo = processStartInfo;
            this.logger = logger;
        }
        public static bool IsInWindowsService()
        {
            return !Environment.UserInteractive;
        }
        public ProcessWrapper(Process process, NodeData nodeData, ILogger logger, IConfiguration configuration)
        {
            this.Process = process;
            this.logger = logger;
            this.NodeData = nodeData;
            nodeData.Argv = nodeData.Argv ?? new string[] { };

            this.ServiceInfo = new ServiceInfo
            {
                Path = nodeData.Argv.Length > 0 ? nodeData.Argv[0] : null,
                Args = nodeData.Argv.Skip(1).Aggregate((a, b) => a + " " + b),
                Name = nodeData.Name
            };


            this.ProcessStartInfo = new ProcessStartInfo
            {
                FileName = nodeData.Argv.Length > 0 ? nodeData.Argv[0] : null,
                Arguments = nodeData.Argv.Skip(1).Aggregate((a, b) => a + " " + b),

            };

        }
        public ProcessWrapper(ServiceInfo info, ILogger logger, IConfiguration configuration)
        {
            this.Name = info.Name;
            this.logger = logger;
            this.ServiceInfo = info;
            var path = Path.GetFullPath(configuration["ServicePath"]);
            var path461 = Path.GetFullPath(configuration["ServicePath461"]);
            this.ProcessStartInfo = new ProcessStartInfo
            {
                FileName = info.Path.Replace("$ServicePath461", path461).Replace("$ServicePath", path),
                Arguments = info.Args,
                UseShellExecute = !IsInWindowsService(),
                CreateNoWindow = false,
                RedirectStandardOutput = IsInWindowsService()


            };

            this.ProcessStartInfo.WorkingDirectory = Path.GetDirectoryName(this.ProcessStartInfo.FileName);
        }
        public async Task Visit()
        {
            if (this.Process != null && this.Process.HasExited)
            {
                //this.logger.LogWarning("Exited...");
            }
        }
        public async Task Start()
        {
            var completion = new TaskCompletionSource<bool>();
            try
            {
                if (this.Process == null)
                {
                    this.Process = new Process();
                    this.Process.StartInfo = this.ProcessStartInfo;

                    this.Process = Process.Start(this.ProcessStartInfo);
                }

                await Task.Delay(2000);
                if (this.Process.HasExited)
                {
                    string output = this.Process.StandardOutput.ReadToEnd();
                    this.logger.LogWarning($"--{output}");
                    completion.SetException(new Exception($"Failed to start application. Process exited"));
                }
                else
                {
                    this.logger.LogInformation(
                        $"Process Successfully Started: {this}");
                    completion.SetResult(true);
                }
                await completion.Task;
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to start process :{this.ToString()}. Error: {err.GetBaseException().Message}");
            }

        }
        public async Task Stop()
        {
            var result = new TaskCompletionSource<bool>();
            var path = this.ProcessStartInfo.WorkingDirectory;
            var file = Path.Combine(path, $"_EXIT_{this.Process.Id}");
            this.logger.LogInformation(
                $"Sending Shudown Signal With File:{file}");
            File.WriteAllText(file, "Shut Down");
            var trials = 0;
            while (trials < 10)
            {
                trials++;
                if (this.Process.HasExited)
                {
                    break;
                }
                await Task.Delay(2000);
            }
            if (this.Process.HasExited)
            {
                result.SetResult(true);
            }
            else
            {
                result.SetException(new Exception(
                    $"Failed to shutdown Process:{this}"));
            }
            await result.Task;

        }
        public override string ToString()
        {
            return $"{this.Name}";
        }

    }
}
