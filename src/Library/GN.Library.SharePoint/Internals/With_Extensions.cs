using GN.Library.SharePoint.Internals;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.SharePoint
{
    public static partial class SharePointExtensions
    {
        private static Expression<Func<TInput, object>> AddBox<TInput, TOutput>(Expression<Func<TInput, TOutput>> expression)
        {
            // Add the boxing operation, but get a weakly typed expression
            Expression converted = Expression.Convert
                 (expression.Body, typeof(object));
            // Use Expression.Lambda to get back to strong typing
            return Expression.Lambda<Func<TInput, object>>
                 (converted, expression.Parameters);
        }
        public static async Task<T2> GetPropertyAsync<T, T2>(this T o, Expression<Func<T, T2>> selector) where T : ClientObject
        {
            var prop = ((selector.Body as MemberExpression).Member) as PropertyInfo;
            var exp = AddBox(selector);
            var is_loaded = typeof(ClientObject).IsAssignableFrom(typeof(T2))
                ? (o.IsObjectPropertyInstantiated(prop.Name))
                : o.IsPropertyAvailable(prop.Name);
            if (!is_loaded)
            {
                o.Context.Load(o, exp);
                await o.Context.ExecuteQueryAsync();
                is_loaded = typeof(ClientObject).IsAssignableFrom(typeof(T2))
                    ? (o.IsObjectPropertyInstantiated(prop.Name))
                    : o.IsPropertyAvailable(prop.Name);
            }
            if (!is_loaded)
            {
                return default(T2);
            }
            return (T2)prop.GetValue(o);
        }
        public static T2 GetProperty<T, T2>(this T o, Expression<Func<T, T2>> selector) where T : ClientObject
        {
            var prop = ((selector.Body as MemberExpression).Member) as PropertyInfo;
            var exp = AddBox(selector);
            var is_loaded = typeof(ClientObject).IsAssignableFrom(typeof(T2))
                ? (o.IsObjectPropertyInstantiated(prop.Name))
                : o.IsPropertyAvailable(prop.Name);
            if (!is_loaded)
            {
                o.Context.Load(o, exp);
                o.Context.ExecuteQuery();
                is_loaded = typeof(ClientObject).IsAssignableFrom(typeof(T2))
                    ? (o.IsObjectPropertyInstantiated(prop.Name))
                    : o.IsPropertyAvailable(prop.Name);
            }
            if (!is_loaded)
            {
                return default(T2);
            }
            return (T2)prop.GetValue(o);
        }
        public static WithCollection<T> WithCollection<T>(this ClientContext context,
            Func<ClientContext, ClientObjectCollection<T>> client,
             params Expression<Func<T, object>>[] selector) where T : ClientObject
        {
            return new WithCollection<T>(client(context), selector);
        }
        public static WithCollection<T> WithCollectionEx<T>(
                this ClientObjectCollection<T> obj,
                params Expression<Func<T, object>>[] selector)
                where T : ClientObject

        {
            return new WithCollection<T>(obj, selector);
        }

        public static WithCollection<T> WithCollectionEx<T, TO>(
            this TO obj,
            Func<TO, ClientObjectCollection<T>> accessor,
            params Expression<Func<T, object>>[] selector)
            where T : ClientObject
            where TO : ClientObject

        {
            return new WithCollection<T>(accessor(obj), selector);
        }

        public static With<T> With<T>(this T client, params Expression<Func<T, object>>[] selector) where T : ClientObject
        {
            selector = selector ?? new Expression<Func<T, object>>[] { };
            return new With<T>(client, selector/*Select(x => AddBox(x)).ToArray()*/);
        }

    }
}
