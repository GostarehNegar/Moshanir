using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GN.Library.SharePoint;
using Microsoft.SharePoint.Client;
using Mapna.Transmittals.Exchange.GhodsNiroo.SharePoint;
using Mapna.Transmittals.Exchange.GhodsNiroo.Incoming;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Mapna.Transmittals.Exchange.GhodsNiroo
{
    public class IncomingTransmittalContext : IDisposable
    {
        private IServiceScope scope;
        private GhodsNirooSharePointContext _spContext;
        private readonly IServiceProvider serviceProvider;
        private GhodsNirooTransmittalOptions options;
        private ILogger logger;
        private CancellationTokenSource CancellationTokenSource;
        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        public IServiceProvider ServiceProvider => this.scope.ServiceProvider;

        public string Id => $"{Request?.Project_Name}/{Request.TR_NO}";
        public string Title => $"Receiving Transmittal '{Request.TR_NO}'";

        public IncomingTransmittalRequest Request { get; }

        public IncomingTransmittalContext(IServiceProvider serviceProvider, GhodsNirooTransmittalOptions options, IncomingTransmittalRequest request)
        {
            this.scope = serviceProvider.CreateScope();
            this.serviceProvider = serviceProvider;
            this.options = options;
            this.Request = request;
            this.logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger($"IncomingTransmittal [{request?.TR_NO}]");
        }

        public IncomingTransmittalContext WithCancellationToken(CancellationTokenSource cancellationToken)
        {
            this.CancellationTokenSource = cancellationToken;
            return this;
        }
        public IncomingTransmittalContext WithOptions(GhodsNirooTransmittalOptions options)
        {
            this.options = options;
            return this;
        }
        public IncomingTransmittalContext WithServiceProvider(IServiceProvider serviceProvider)
        {
            this.scope = serviceProvider.CreateScope();
            return this;
        }
        internal GhodsNirooSharePointContext GetSPContext(bool refersh = false)
        {
            if (this._spContext == null || refersh)
            {
                this._spContext = new GhodsNirooSharePointContext(ServiceProvider.GetService<IClientContextFactory>()
                     .CreateContext(this.options.ConnectionString));
            }
            return this._spContext;
        }

        public void Log(LogLevel level, string message, params object[] args)
        {
            this.logger.Log(level, message, args);
            _ = this.GetSPContext().SendLogAsync(level, this.Request?.TR_NO, message, args);
        }

        public string Serialize(object data)
        {
            return System.Text.Json.JsonSerializer.Serialize(data);
        }
        public void Dispose()
        {
            this.scope?.Dispose();
            this._spContext?.Dispose();
            this.CancellationTokenSource?.Dispose();
        }
        public override string ToString()
        {
            return Title;
        }
    }
}
