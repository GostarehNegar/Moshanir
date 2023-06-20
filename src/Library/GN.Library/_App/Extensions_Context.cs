using GN.Library;
using GN.Library.Data;
using System;
using System.Collections.Generic;
using System.Text;
using GN.Library.Shared.Entities;

namespace GN
{
    public static partial class Extensions
    {
        public static ILocalDocumentStore Local(this IAppDataServices This) => This.AppContext.GetService<ILocalDocumentStore>();
        public static IPublicDocumentStore Public(this IAppDataServices This) => This.AppContext.GetService<IPublicDocumentStore>();
        public static IUserDocumentStore User(this IAppDataServices This) => This.AppContext.GetService<IUserDocumentStore>();
        public static IDynamicEntityRepository<TEntity> DynamicRepository<TEntity>(this IAppDataServices This) where TEntity : DynamicEntity, new()
        {
            return This.AppContext.GetService<IDynamicEntityRepository<TEntity>>();

        }
    }
}
