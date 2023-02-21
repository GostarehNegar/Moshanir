using GN.Library.Natilus.Internals;
using Microsoft.Extensions.Hosting;
using NATS.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO.Compression;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GN.Library.Natilus
{
    public interface INatilusConnectionProvider
    {
        Task<IConnection> GetConnectionAsync(string name = "default", bool refersh = false);

    }
    class NatilusConnectionProvider : IHostedService, INatilusConnectionProvider,IHealthCheck
    {
        private ConcurrentDictionary<string, IConnection> connections = new ConcurrentDictionary<string, IConnection>();
        private Process natsProcess;
        private readonly ILogger<NatilusConnectionProvider> logger;
        private readonly NatilusOptions natilusOptions;

        private Options Options => ConnectionFactory.GetDefaultOptions();
        public NatilusConnectionProvider(ILogger<NatilusConnectionProvider> logger, NatilusOptions options)
        {
            this.logger = logger;
            this.natilusOptions = options;
        }
        private async Task<IConnection> AutoStart(bool retry = false)
        {
            var zipFileName = "nats-server.zip";
            var nats_server = "nats-server.exe";
            var nats_config = "nats.config.yaml";
            
            try
            {
                var nats_server_fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), nats_server));
                var nats_config_fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), nats_config));
                var nats_zip_fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), zipFileName));

                if (this.natsProcess == null || retry)
                {
                    this.natsProcess = new Process();
                    if (!File.Exists(nats_server_fullPath))
                    {
                        var name = this.GetType().Assembly.GetManifestResourceNames()
                            .FirstOrDefault(x => x.ToLowerInvariant().EndsWith(zipFileName));
                        if (!string.IsNullOrWhiteSpace(name))
                        {

                            using (var stream = this.GetType().Assembly.GetManifestResourceStream(name))
                            {
                                if (File.Exists(nats_zip_fullPath))
                                {
                                    File.Delete(nats_zip_fullPath);
                                }
                                using (var zipFile = System.IO.File.OpenWrite(nats_zip_fullPath))
                                {
                                    await stream.CopyToAsync(zipFile);
                                    await stream.FlushAsync();
                                    await zipFile.FlushAsync();
                                }
                            }
                            var archive = ZipFile.Open(nats_zip_fullPath, ZipArchiveMode.Read)
                                .Entries.FirstOrDefault(x => x.Name == nats_server);
                            archive.ExtractToFile(nats_server_fullPath, true);
                        }
                    }
                    if (File.Exists(nats_server_fullPath))
                    {
                        var startInfo = new ProcessStartInfo();
                        startInfo.FileName = nats_server_fullPath;
                        startInfo.UseShellExecute = false;
                        if (File.Exists(nats_config_fullPath))
                        {
                            startInfo.Arguments = $"-c {nats_config_fullPath}";
                        }
                        else
                        {

                        }
                        this.natsProcess.StartInfo = startInfo;
                        this.natsProcess.ErrorDataReceived += (a, b) =>
                        {

                        };
                        this.natsProcess.Start();
                        await Task.Delay(1000);
                    }
                }
                return await this.CreateConnection(false);
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to AutoStart nats-server. Err:{err.Message}");
            }
            return null;
        }
        private async Task<IConnection> CreateConnection(bool autoStart = false, int timeOut = 2000)
        {
            try
            {
                this.Options.Timeout = timeOut;
                return new ConnectionFactory().CreateConnection(this.Options);
            }
            catch (Exception err)
            {
                if (autoStart)
                {
                }

            }
            if (autoStart)
            {
                return await AutoStart();
            }

            return null;
        }
        public async Task<IConnection> GetConnectionAsync(string name = "default", bool refersh = false)
        {
            if (!connections.TryGetValue(name, out var result) || refersh)
            {
                if (result != null && !result.IsClosed())
                    await result?.DrainAsync();
                result = await CreateConnection(true);
                connections[name] = result;
            }
            return result;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var conn = await this.GetConnectionAsync("health", true);
            try
            {
                if (conn != null)
                {
                    return context.Healthy("NATS Connection")
                        .WriteLine($"Url:'{conn.ConnectedUrl}', Id:'{conn.ConnectedId}' ");
                }
                else
                {
                    var op = this.Options;
                    return context.Unhealthy("NATS Connection")
                        .WriteLine($"Url:'{op.Url}'");
                }
            }
            finally
            {
                conn?.Dispose();
            }
        }
    }
}
