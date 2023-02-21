using System;

namespace GN.Library.Functional.Pipelines
{
    public class PipeEventArgs
    {
        public PipelineEvents Event { get; }
        public IPipelineContext Context { get; }
        public PipelineStepInfo Step { get; }
        public Exception LastError { get; }
        internal PipeEventArgs(PipelineEvents ev, IPipelineContext context, Exception lastError, PipelineStepInfo step = null)
        {
            this.Event = ev;
            this.Context = context;
            this.LastError = lastError;
            this.Step = step;
        }
        public override string ToString()
        {
            return $"Event: '{Event}' in step: '{this.Step?.Name}'";
        }
        internal static PipeEventArgs Completed(IPipelineContext context)
        {
            return new PipeEventArgs(PipelineEvents.Completed, context, null);
        }
        internal static PipeEventArgs StepStart(IPipelineContext context, PipelineStepInfo step)
        {
            return new PipeEventArgs(PipelineEvents.Step, context, null, step);
        }
        internal static PipeEventArgs Error(IPipelineContext context, PipelineStepInfo step, Exception exception)
        {
            return new PipeEventArgs(PipelineEvents.Error, context, exception, step);
        }
    }
}
