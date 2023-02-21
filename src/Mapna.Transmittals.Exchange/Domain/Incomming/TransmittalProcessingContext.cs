using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using Mapna.Transmittals.Exchange.Services.Queues;

namespace Mapna.Transmittals.Exchange.Services.Queues
{
    public class FileState
    {
        public string Url { get; set; }
        public string State { get; set; }
    }
    public class ProcessingState
    {
        public ConcurrentDictionary<string, FileState> Files { get; set; }

        public ProcessingState SetFileState(string Url, string state)
        {
            this.Files = this.Files ?? new ConcurrentDictionary<string, FileState>();
            this.Files.AddOrUpdate(Url, new FileState { Url = Url, State = state }, (a, b) => { b.State = state; return b; });
            return this;
        }
    }
    public class TransmittalProcessingContext : TransmittalProcessingContext<QueueContextBase>
    {
    }
    public class TransmittalProcessingContext<T> : QueueContextBase<T> where T : QueueContextBase
    {
        public virtual string Title => this.Transmittal?.TR_NO;
        public ProcessingState State { get; set; } = new ProcessingState();
        public Exception LastException { get; internal set; }
        //public CancellationToken CancellationToken => this.cancellation.Token;
        public TransmittalSubmitModel Transmittal { get; set; }
        public string Id => this.Transmittal.GetInternalId();
        public TransmittalProcessingContext()
        {
            this.cancellation = new CancellationTokenSource();
        }


        public new void Dispose()
        {
            base.Dispose();
            //this.cancellation?.Dispose();
            //this.scope?.Dispose();

        }

        private T Self => this as T;
        internal new Task<T> SetCannellationToken(CancellationToken token)
        {
            base.SetCannellationToken(token);
            return Task.FromResult(Self);
        }
        public T Validate()
        {
            this.MaxTrials = this.MaxTrials > 100 ? 100 : this.MaxTrials;

            return Self;
        }

    }


}
