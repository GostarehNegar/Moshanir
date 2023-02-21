using Microsoft.SharePoint.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.SharePoint.Internals
{
    public class WithCollection<T> : IQueryable<T> where T : ClientObject
    {
        private readonly ClientObjectCollection<T> clientObject;
        private readonly List<Expression<Func<T, object>>> selectors;

        private void load()
        {
            var retreivals = selectors.ToArray();
            this.clientObject.Context.Load(this.clientObject, c => c.Include(retreivals));
            //this.clientObject.Context.Load(this.clientObject, c => c.IncludeWithDefaultProperties(retreivals));
            this.clientObject.Context.ExecuteQuery();

        }
        public WithCollection(ClientObjectCollection<T> clientObject, params Expression<Func<T, object>>[] selector)
        {
            this.clientObject = clientObject;
            this.selectors = new List<Expression<Func<T, object>>>(selector ?? new Expression<Func<T, object>>[] { });

            load();
        }
        public WithCollection<T> Add(params Expression<Func<T, object>>[] selector)
        {
            this.selectors.AddRange(selector ?? new Expression<Func<T, object>>[] { });
            load();
            return this;
        }
        public IQueryable<T> Querayble => this.clientObject;

        public Expression Expression => Querayble.Expression;

        public Type ElementType => Querayble.ElementType;

        public IQueryProvider Provider => Querayble.Provider;
        public WithCollection<T> Include(params Expression<Func<T, object>>[] args)
        {

            this.clientObject
                .ToList()
                .ForEach(x => new With<T>(x, args));
            return this;
        }

        public WithCollection<T> ForEach(Action<With<T>> action, params Expression<Func<T, object>>[] args)
        {
            this.clientObject
                .ToList()
                .ForEach(x => action?.Invoke(new With<T>(x, args)));
            return this;
        }

        public IEnumerator<T> GetEnumerator() => Querayble.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Querayble.GetEnumerator();

        //System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
