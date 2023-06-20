using GN.Library.Functional;
using Mapna.Transmittals.Exchange.Internals;
using Mapna.Transmittals.Exchange.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Mapna.Transmittals.Exchange.Domain.Outgoing
{
    public class TransmittalOutgiongContext : IDisposable
    {
        public ConcurrentDictionary<string, object> Values = new ConcurrentDictionary<string, object>();
        public IServiceScope Scope { get; set; }
        public IServiceProvider ServiceProvider => Scope.ServiceProvider;
        public CancellationToken CancellationToken { get; set; }

        public WithPipe<TransmittalOutgiongContext> Pipe { get; set; }
        public string TransimttalNumber { get; set; }
        private ILogger logger;

        public ITransmittalRepository GetRepository() => this.ServiceProvider.GetRequiredService<ITransmittalRepository>();

        internal SPTransmittalItem SPTransmittalItem { get; set; }
        public TransmittalOutgoingModel Transmittal { get; set; }
        public void Dispose()
        {
            this.Scope.Dispose();
        }

        public ILogger GetLogger()
        {
            this.logger = this.logger ?? this.ServiceProvider.GetService<ILoggerFactory>().CreateLogger("OutGoing");
            return this.logger;
        }
        public void SendLog( LogLevel level, string message, params object[] args)
        {
            GetLogger().Log(level, message, args);
            if (level >= LogLevel.Information)
            {
                GetRepository().SendLog(level, message, args);
            }

        }
    }
}
