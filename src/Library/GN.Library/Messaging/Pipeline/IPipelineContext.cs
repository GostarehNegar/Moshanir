using GN.Library.Messaging.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Messaging.Pipeline
{
    public enum Pipelines
    {
        Incomming,
        Outgoing,
    }
    public interface IPipelineContext
    {
        IDictionary<string, object> Properties { get; }
        IMessageContext MessageContext { get; }
        IMessagingServices Services { get; }
        IPipelineStep[] Steps { get; }
        Task Invoke();
        CancellationToken CancellationToken { get; }
        void RaiseError(Exception err);
    }
    class PipelineContext : IPipelineContext
    {
        private Dictionary<string, object> properties = new Dictionary<string, object>();
        private TaskCompletionSource<bool> TaskCompletionSource;
        public IMessageContext MessageContext { get; private set; }

        public IMessagingServices Services { get; private set; }

        public IPipelineStep[] Steps { get; private set; }

        public CancellationToken CancellationToken { get; set; }

        public IDictionary<string, object> Properties => this.properties;
        public Task Invoke()
        {
            Task do_invoke(int idx)
            {
                if (idx == this.Steps.Length)
                {
                    this.TaskCompletionSource.SetResult(true);
                    return Task.CompletedTask;
                }
                try
                {
                    return this.Steps[idx].Handle(this, ctx =>
                    {
                        return do_invoke(idx + 1);
                    });
                }
                catch(Exception err)
                {
                    this.TaskCompletionSource.SetException(err);
                    throw;
                }
            }
            return do_invoke(0);
        }

        public Task Invoke0()
        {
            Task do_invoke(int idx)
            {
                if (idx == this.Steps.Length)
                {
                    this.TaskCompletionSource.SetResult(true);
                    return Task.CompletedTask;
                }
                try
                {
                    return this.Steps[idx].Handle(this, ctx =>
                    {
                        return do_invoke(idx + 1);
                    });
                }
                catch (Exception err)
                {
                    this.TaskCompletionSource.SetException(err);
                    throw;
                }
            }
            return do_invoke(0);
        }

        public void RaiseError(Exception err)
        {

        }

        public PipelineContext(IMessagingServices services, IMessageContext message, IPipelineStep[] Steps)
        {
            this.Services = services;
            this.MessageContext = message;
            this.Steps = Steps;
            this.TaskCompletionSource = new TaskCompletionSource<bool>();
        }
        public Task CompletionTask => this.TaskCompletionSource.Task;
        


    }
    public interface IPipelineStep
    {
        Task Handle(IPipelineContext ctx, Func<IPipelineContext, Task> next);
    }
    class PipelineStep : IPipelineStep
    {
        private Func<IPipelineContext, Func<IPipelineContext, Task>, Task> handler;
        public PipelineStep(Func<IPipelineContext, Func<IPipelineContext, Task>, Task> handler)
        {
            this.handler = handler;
        }

        public Task Handle(IPipelineContext ctx, Func<IPipelineContext, Task> next)
        {
            return this.handler(ctx, next);
        }
    }
}
