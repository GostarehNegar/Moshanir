using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.SharePoint.Internals
{
    public class With<T> where T : ClientObject
    {
        private readonly T clientObject;
        private readonly List<Expression<Func<T, object>>> selector = new List<Expression<Func<T, object>>>();

        //public T ClientObject => clientObject;
        public Task<T> ClientObject => loadAsync();
        private static Expression<Func<TInput, object>> AddBox<TInput, TOutput>
        (Expression<Func<TInput, TOutput>> expression)
        {
            // Add the boxing operation, but get a weakly typed expression
            Expression converted = Expression.Convert
                 (expression.Body, typeof(object));
            // Use Expression.Lambda to get back to strong typing
            return Expression.Lambda<Func<TInput, object>>
                 (converted, expression.Parameters);
        }
        private void load()
        {

            var retr = selector.ToList()
                .Where(r =>
                {
                    var is_loaded = true;
                    if (r.Body as MemberExpression != null)
                    {
                        var prop = ((r.Body as MemberExpression).Member) as PropertyInfo;
                        is_loaded = typeof(ClientObject).IsAssignableFrom(prop.PropertyType)
                                ? (this.clientObject.IsObjectPropertyInstantiated(prop.Name))
                                : this.clientObject.IsPropertyAvailable(prop.Name);

                    }
                    else if ((r.Body as UnaryExpression)?.Operand as MemberExpression != null)
                    {
                        var exp = (r.Body as UnaryExpression).Operand as MemberExpression;
                        is_loaded = this.clientObject.IsPropertyAvailable(exp.Member.Name);
                    }
                    return !is_loaded;
                })
                .ToArray();
            if (retr.Length > 0)
            {
                this.clientObject.Context.Load(this.clientObject, retr);
                this.clientObject.Context.ExecuteQuery();
            }

        }
        private async Task<T> loadAsync()
        {

            var retr = selector.ToList()
                .Where(r =>
                {
                    var is_loaded = true;
                    if (r.Body as MemberExpression != null)
                    {
                        var prop = ((r.Body as MemberExpression).Member) as PropertyInfo;
                        is_loaded = typeof(ClientObject).IsAssignableFrom(prop.PropertyType)
                                ? (this.clientObject.IsObjectPropertyInstantiated(prop.Name))
                                : this.clientObject.IsPropertyAvailable(prop.Name);

                    }
                    else if ((r.Body as UnaryExpression)?.Operand as MemberExpression != null)
                    {
                        var exp = (r.Body as UnaryExpression).Operand as MemberExpression;
                        is_loaded = this.clientObject.IsPropertyAvailable(exp.Member.Name);
                    }
                    return !is_loaded;
                })
                .ToArray();
            if (retr.Length > 0)
            {
                this.clientObject.Context.Load(this.clientObject, retr);
                await this.clientObject.Context.ExecuteQueryAsync();
            }
            return this.clientObject;

        }
        public With(T clientObject, params Expression<Func<T, object>>[] selector)
        {
            this.clientObject = clientObject;
            this.Add(selector);
        }
        public With<T> Add(params Expression<Func<T, object>>[] selector)
        {
            var s = selector ?? new Expression<Func<T, object>>[] { }
                .Select(x => x)
                .ToArray();
            this.selector.AddRange(s);
            //load();
            return this;
        }
        public async Task<With<T>> DoAsync(Action<T> action)
        {
            action?.Invoke(await this.ClientObject);
            return this;
        }
        public async Task<With<T>> DoAsync(Func<T, Task> action)
        {
            await (action == null ? Task.CompletedTask : action(await this.ClientObject));
            return this;
        }
        public async Task<TResult> DoAsync<TResult>(Func<T, TResult> action)
        {
            return action == null ? default(TResult) : action(await this.ClientObject);


        }
        public async Task<TResult> DoAsync<TResult>(Func<T, Task<TResult>> action)
        {
            return await (action == null ? Task<TResult>.FromResult(default(TResult)) : action(await this.ClientObject));
        }

        public With<T> Do(Action<T> action)
        {
            this.load();
            action?.Invoke(this.clientObject);
            return this;

        }
        public TRes Do<TRes>(Func<T, TRes> action)
        {
            this.load();
            return action == null ? default(TRes) : action.Invoke(this.clientObject);
        }
        //public async Task<TResult> Return<TResult>(Func<T, TResult> func)
        //{

        //    return func != null ? func( await this.ClientObject) : default(TResult);
        //}
        //public async Task<TResult> Return<TResult>(Func<T, Task<TResult>> func)
        //{

        //    return func != null ? await func(await this.ClientObject) : default(TResult);
        //}

        public bool IsQuerable => this.clientObject as IQueryable<T> != null;
        public With<T> ForEach<TO>(Action<TO> action)
        {
            (this.clientObject as ClientObjectCollection<TO>)
                .ToList()
                .ForEach(x => action?.Invoke(x));
            return this;
        }

    }
}
