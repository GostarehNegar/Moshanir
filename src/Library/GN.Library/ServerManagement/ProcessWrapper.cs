using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace GN.Library.ServerManagement
{ 
    class ProcessWrapper
    {
        private readonly ILogger logger;
        public Process Process { get; private set; }

        public string Name { get; }
        public ProcessStartInfo ProcessStartInfo { get; }
        

        public ProcessWrapper(string Name, ProcessStartInfo processStartInfo, ILogger logger)
        {
            this.Name = Name;
            ProcessStartInfo = processStartInfo;
            this.logger = logger;
        }
        public ProcessWrapper(ServiceInfo info, ILogger logger, IConfiguration configuration)
        {
            this.Name = info.Name;
            this.logger = logger;
            var path = configuration["ServicePath"];
            var path461 = configuration["ServicePath461"];
            this.ProcessStartInfo = new ProcessStartInfo
            {
                FileName = info.Path.Replace("$ServicePath461", path461).Replace("$ServicePath", path),
                Arguments = info.Args
            };
            this.ProcessStartInfo.WorkingDirectory = Path.GetDirectoryName(this.ProcessStartInfo.FileName);
        }
        public async Task Visit()
        {
            if (this.Process.HasExited)
            {
                this.logger.LogWarning("Exited...");
            }
        }
        public async Task Start()
        {
            var completion = new TaskCompletionSource<bool>();
            try
            {
                if (this.Process == null)
                {
                    this.Process = Process.Start(this.ProcessStartInfo);
                }
                await Task.Delay(2000);
                if (this.Process.HasExited)
                {
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
            while (trials<10)
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
