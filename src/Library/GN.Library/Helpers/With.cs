using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace GN.Library.Helpers
{
    public class With<T>
    {
        private Func<T, T> pipe = t => t;
        public With()
        {
        }
        public With<T> First(Func<T, Func<T, T>, T> step)
        {
            var p = this.pipe;
            this.pipe = t =>
            {
                return step(t, p);
            };
            return this;
        }
        public With<T> Then(Func<T, Func<T, T>, T> step)
        {
            var p = this.pipe;
            this.pipe = t =>
            {
                return step(p(t), a => a);
            };
            return this;
        }
        public T Run(T target)
        {
            return this.pipe(target);
        }
        public static With<T> Setup() => new With<T>();
    }

    public class WithAsync<T>
    {
        private Func<T, Task<T>> pipe = t => Task.FromResult(t);
        private Func<T, Task<T>> _finally = null;
        public WithAsync()
        {
        }
        public WithAsync<T> Finally(Func<T, Task<T>> step)
        {
            this._finally = step;
            return this;
        }

        public WithAsync<T> First(Func<T, Func<T, Task<T>>, Task<T>> step)
        {
            var p = this.pipe;
            this.pipe = t =>
            {
                return step(t, p);
            };
            return this;
        }
        public WithAsync<T> First(Func<T, Task<T>> step)
        {
            var p = this.pipe;
            this.pipe = t =>
            {
                return step(t);
            };
            return this;
        }
        public WithAsync<T> First1(Func<T, Task> step)
        {
            var p = this.pipe;
            this.pipe = async t =>
            {
                await step(t);
                return t;
            };
            return this;
        }
        public WithAsync<T> Then(Func<T, Func<T, Task<T>>, Task<T>> step)
        {
            var p = this.pipe;
            this.pipe = async t =>
            {
                return await step(await p(t), a => Task.FromResult(a));
            };
            return this;
        }
        public WithAsync<T> Then(Func<T, Task<T>> step)
        {
            var p = this.pipe;
            this.pipe = async t =>
            {
                return await step(await p(t));
            };
            return this;
        }
        public WithAsync<T> Then1(Func<T, Task> step)
        {
            var p = this.pipe;
            this.pipe = async t =>
            {
                await step(await p(t));
                return t;
            };
            return this;
        }
        public async Task<T> Run(T target)
        {
            T result = default;
            try
            {
                result = await this.pipe(target);
                return result;
            }
            finally
            {
                if (this._finally != null)
                {
                    await this._finally(target);
                }
            }
        }
        public WithAsync<T> RunAndContinueWith(T target, Action<Task<T>> action)
        {
            var t = this.Run(target).ContinueWith(f1 =>
            {
                action(f1);
                return f1;

            });
            return WithAsync<T>.Setup()
                .First(async ctx =>
                {
                    await t;
                    var gggg = t.Result;
                    return target;
                });

        }
        public static WithAsync<T> Setup() => new WithAsync<T>();
    }

    public class WithPipeEx<T>
    {
        private Func<T, Func<T, Task>, Task> pipe = (x, n) => n(x);
        private List<Func<T, Func<T, Task>, Task>> steps = new List<Func<T, Func<T, Task>, Task>>();
        public WithPipeEx<T> Then(Func<T, Func<T, Task>, Task> step)
        {
            this.steps.Add(step);

            return this;
        }
        public async Task<T> Run(T context)
        {
            Task do_invoke(int idx)
            {
                if (idx == this.steps.Count)
                    return Task.CompletedTask;
                return this.steps[idx](context, ctx =>
                {
                    return do_invoke(idx + 1);
                });
            }
            await do_invoke(0);
            return context;
        }

        public static WithPipeEx<T> Setup() => new WithPipeEx<T>();
    }
}

