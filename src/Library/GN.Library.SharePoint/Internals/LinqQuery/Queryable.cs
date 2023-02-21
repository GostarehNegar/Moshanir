using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.SharePoint.Internals.LinqQuery
{
    internal class Queryable<T> : IOrderedAsyncQueryable<T>
    {

        internal Queryable(IQueryExecutor queryContext)
        {
            Initialize(new QueryProvider(queryContext), null);
            //Initialize(new Queryable<T>())
        }


        public Queryable(IAsyncQueryProvider provider)
        {
            Initialize(provider, null);
        }

        internal Queryable(IAsyncQueryProvider provider, Expression expression)
        {
            Initialize(provider, expression);
        }

        private void Initialize(IAsyncQueryProvider provider, Expression expression)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            if (expression != null && !typeof(IAsyncQueryable<T>).
                   IsAssignableFrom(expression.Type))
                throw new ArgumentException(
                     String.Format("Not assignable from {0}", expression.Type), "expression");

            Provider = provider;
            Expression = expression ?? Expression.Constant(this);
        }

        //public IEnumerator<T> GetEnumerator()
        //{
        //    return (Provider.Execute<IEnumerable<T>>(Expression)).GetEnumerator();
        //}

        //System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //{
        //    return (Provider.Execute<System.Collections.IEnumerable>(Expression)).GetEnumerator();
        //}

        //public async IAsyncEnumerable<T> HHH(CancellationToken cancellationToken = default)
        //{
        //    for (int i = 0; i < 2; i++)
        //    {
        //        yield return default(T);
        //    }
        //}

        public Task<IEnumerable<T>> ToArrayAsync()
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public Expression Expression { get; private set; }
        public IAsyncQueryProvider Provider { get; private set; }

        //IAsyncQueryProvider IAsyncQueryable.Provider => throw new NotImplementedException();
    }
}
