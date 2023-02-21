using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Helpers
{
    public class WithAsyncEx<T, Tin>
    {
        public class Context
        {
            public ConcurrentDictionary<string, object> Properties = new ConcurrentDictionary<string, object>();
            public Context()
            {

            }
            public Exception Exception { get; internal set; }

            public Context SetProperty(string key, object value)
            {
                this.Properties.AddOrUpdate(key, value, (a, b) => value);
                return this;
            }
            public TO GetProperty<TO>(string key)
            {
                return this.Properties.TryGetValue(key, out var o) ? (TO)o : default(TO);
            }

        }
        private Context context = new Context();
        private Func<T, Exception, Context, Task<T>> excptionHandler;
        private Func<T, Context, Task> finaly;
        private Func<T, Task<T>> pipe = t => Task.FromResult(t);
        private Func<Tin, T> converter;
        private T latest;
        private Tin input;
        public WithAsyncEx(Func<Tin, T> converter, Context context = null, Func<T, Task<T>> start = null)
        {
            this.converter = converter;
            this.pipe = start == null ? (t => Task.FromResult(t)) : start;
            this.context = context ?? new Context();
        }

        public WithAsyncEx<T, Tin> Then(Func<T, Task<T>> step)
        {
            var p = this.pipe;
            this.pipe = async t =>
            {
                try
                {
                    return await step(await p(t));
                }
                catch (Exception err)
                {
                    if (this.excptionHandler != null)
                    {
                        this.context.Exception = err;
                        return await this.excptionHandler(t, err, this.context);
                    }
                    else
                    {
                        throw;
                    }
                }
            };
            return this;
        }
        public WithAsyncEx<T, Tin> Then(Func<T, Context, Task<T>> step)
        {
            return this.Then(t => step(t, this.context));
        }

        public WithAsyncEx<T, Tin> ThenVoid(Func<T, Task> step)
        {
            return Then(async x => { await step(x); return x; });
        }

        public WithAsyncEx<T, Tin> Then(Action<T> action)
        {
            this.Then(x =>
            {
                action?.Invoke(x);
                return Task.FromResult(x);
            });
            return this;
        }
        public WithAsyncEx<T, Tin> Then(Func<T, T> action)
        {
            this.Then(x =>
            {
                if (action != null)
                {
                    x = action.Invoke(x);
                }
                return Task.FromResult(x);
            });
            return this;
        }
        public WithAsyncEx<T, Tin> Except(Func<T, Exception, Context, Task<T>> handler)
        {
            this.excptionHandler = handler;
            return this;
        }
        public WithAsyncEx<T, Tin> Except(Func<T, Exception, Task<T>> handler)
        {
            this.excptionHandler = (t, e, c) => handler(t, e);
            return this;
        }

        public WithAsyncEx<T, Tin> Except(Func<T, Exception, T> handler)
        {
            this.excptionHandler = (t, e, c) => Task.FromResult(handler(t, e));
            return this;
        }
        public WithAsyncEx<T, Tin> Except(Action<T, Exception> handler)
        {
            this.Except((x, e) => { handler?.Invoke(x, e); return x; });
            return this;
        }
        public WithAsyncEx<T, Tin> Except(Action<Exception> handler)
        {
            this.Except((x, e) => handler?.Invoke(e));
            return this;
        }
        public WithAsyncEx<T, Tin> Except(Func<Exception, T> handler)
        {
            this.Except((x, e, c) => Task.FromResult(handler(e)));
            return this;
        }
       

        public WithAsyncEx<T, Tin> Finally(Func<T, Context, Task> handler)
        {
            this.finaly = handler;
            return this;
        }
        public WithAsyncEx<T, Tin> Finally(Func<T, Task> handler)
        {
            this.Finally((x, c) => handler(x));
            return this;
        }
        public WithAsyncEx<T, Tin> Finally(Action<T> handler)
        {
            this.Finally((x, c) => { handler(x); return Task.CompletedTask; });
            return this;
        }
        public WithAsyncEx<TN, Tin> Cast<TN>(Func<Tin, TN> convert)
        {
            var result = new WithAsyncEx<TN, Tin>(convert);
            result.ThenVoid(x => this.Run(result.input, this.context));

            return result;// as WithAsyncVoidEx<TN>;
        }

        public async Task<T> Run(Tin target, Context context = null)
        {
            this.input = target;
            this.context = context ?? new Context();
            T res = default(T);
            try
            {
                res = await this.pipe(this.converter(target));
                return res;
            }
            //catch (Exception err)
            //{
            //    if (this.excptionHandler != null)
            //    {
            //        return await this.excptionHandler(this.latest, err, this.context);
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}
            finally
            {
                if (this.finaly != null)
                {
                    await this.finaly(res, this.context);
                }
            }

            //return result;
        }
    }
    public class WithAsyncExEx<T> : WithAsyncEx<T, T>
    {
        public WithAsyncExEx() : base(x => x)
        {

        }
        public static WithAsyncExEx<T> Setup() => new WithAsyncExEx<T>();
    }
}
