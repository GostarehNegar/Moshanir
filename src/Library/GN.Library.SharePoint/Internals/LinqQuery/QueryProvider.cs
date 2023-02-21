using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.SharePoint.Internals.LinqQuery
{

    internal class QueryProvider : IAsyncQueryProvider
    {
        private readonly IQueryExecutor executor;
        public QueryProvider(IQueryExecutor queryContext)
        {
            this.executor = queryContext;
        }
        public virtual IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return
                   (IQueryable)Activator.CreateInstance(typeof(Queryable<>).
                          MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public virtual IAsyncQueryable<T> CreateQuery<T>(Expression expression)
        {
            return new Queryable<T>(this, expression);
        }
        public async ValueTask<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
        {
            var items = await this.executor.Execute(expression, true);
            Type itemsType = items == null || !items.GetType().IsArray ? null : items.GetType().GetElementType();
            Type resultType = typeof(TResult).IsArray ? typeof(TResult).GetElementType() : typeof(TResult);
            if (!resultType.IsAssignableFrom(itemsType))
            {
                throw new Exception("Invalid Cast.");
            }
            return typeof(TResult).IsArray ? (TResult)(items)
                : ((TResult[])items).Length > 0 ? ((TResult[])items)[0] : default(TResult);
        }
    }
}
