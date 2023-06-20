using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Functional
{
    public class PipeUnrecoverableException : Exception
    {
        public PipeUnrecoverableException(string message, Exception innerException = null) : base(message, innerException) { }
    }
    public interface IWithPipe
    {
        IDictionary<string, object> Parameters { get; }

    }
    public class WithPipe<T> : IWithPipe
    {

        private Func<T, IWithPipe, Func<T, Task>, Task> pipe = (x, p, n) => n(x);
        private List<Func<T, IWithPipe, Func<T, Task>, Task>> steps = new List<Func<T, IWithPipe, Func<T, Task>, Task>>();
        private int trialCounts = 1;
        private ConcurrentDictionary<string, object> parameters = new ConcurrentDictionary<string, object>();

        public IDictionary<string, object> Parameters => this.parameters;

        public WithPipe<T> Then(Func<T, IWithPipe, Func<T, Task>, Task> step)
        {
            this.steps.Add(step);
            return this;
        }
        public WithPipe<T> Then(Func<T, Func<T, Task>, Task> step)
        {
            this.steps.Add((x, p, n) => step(x, n));
            return this;
        }
        public WithPipe<T> Then(Func<T, Task> step)
        {
            this.steps.Add(async (x, p, n) =>
            {
                await step(x);
                await n(x);

            });
            return this;
        }

        public WithPipe<T> Retrials(int count)
        {
            this.trialCounts = count;
            return this;
        }
        public async Task<T> DoRun(T context)
        {
            Task do_invoke(int idx)
            {
                if (idx == this.steps.Count)
                    return Task.CompletedTask;
                return this.steps[idx](context, this, ctx =>
                 {
                     return do_invoke(idx + 1);
                 });
            }
            await do_invoke(0);
            return context;
        }
        public async Task<T> Run(T context, int? trials = null)
        {
            Exception lastError = new Exception("");
            this.trialCounts = trials.HasValue ? trials.Value : this.trialCounts;
            this.trialCounts = this.trialCounts < 1 ? 1 : this.trialCounts;
            for (var i = 0; i < this.trialCounts; i++)
            {
                try
                {
                    return await DoRun(context);

                }
                catch (PipeUnrecoverableException err)
                {
                    throw err.InnerException ?? err;
                }
                catch (Exception err)
                {
                    lastError = err;
                    if (i == this.trialCounts - 1)
                    {
                        throw;
                    }
                }
            }
            throw lastError;
        }


        public static WithPipe<T> Setup(Action<WithPipe<T>> configure=null)
        {
            var result = new WithPipe<T>();
            configure?.Invoke(result);
            return result;
        }


    }
}
