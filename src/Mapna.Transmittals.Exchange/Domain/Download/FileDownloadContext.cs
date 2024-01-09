using System;
using System.Threading.Tasks;
using System.Threading;
using Mapna.Transmittals.Exchange.Services.Queues;

namespace Mapna.Transmittals.Exchange.Internals
{
    public enum DownloadStrategy
    {
        DownloadToLocal,
        DownloadToSharepoint,
    }
    public class FileDownloadContext : QueueContextBase<FileDownloadContext>, IDisposable
    {
        public event EventHandler<FileDownloadContext> OnCompleted;
        public event EventHandler<FileDownloadContext> OnCanceled;
        public event EventHandler<FileDownloadContext> OnError;
        public event EventHandler<FileDownloadContext> OnProgress;
              
        public DownloadStrategy Stratgey { get; set; }

        public string Url { get; set; }
        public bool ProgressSupport { get; set; }

        public string Destination { get; set; }
        public long BytesReceived { get; private set; }
        public long TotalBytesToReceive { get; private set; }
        public long ProgressPercentage => BytesReceived * 100 / (TotalBytesToReceive == 0 ? 1 : TotalBytesToReceive);
        public TransmittalFileSubmitModel FileSubmitModel;
        internal FileDownloadContext(string url, string destination)
        {

            this.Url = url;
            this.Destination = destination;
            this.Stratgey = DownloadStrategy.DownloadToSharepoint;
        }
        
        internal void InvokeCompelted()
        {
            this.OnCompleted?.Invoke(this, this);
        }
        internal void InvokeProgress(long a, long b)
        {
            this.BytesReceived = a;
            this.TotalBytesToReceive = b;
            this.OnProgress?.Invoke(this, this);
        }
        internal void InvokeCanceled()
        {

            this.OnCanceled?.Invoke(this, this);
        }
        internal void InvokeError(Exception err)
        {
            this.OnError?.Invoke(this, this);
        }
        public FileDownloadContext WithStrategy(DownloadStrategy strategy)
        {
            this.Stratgey = strategy;
            return this;
        }
        public FileDownloadContext WithFile(TransmittalFileSubmitModel file)
        {
            this.FileSubmitModel = file;
            return this;
        }


        public new void Dispose()
        {
            base.Dispose();
            this.cancellation?.Dispose();
        }

       
    }
}
