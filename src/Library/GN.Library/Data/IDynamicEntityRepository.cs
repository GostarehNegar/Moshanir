using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace GN.Library.Data
{
    public interface IDynamicEntityRepository<TEntity> where TEntity : DynamicEntity, new()
    {
        TEntity GetOrAddOrUpdate(string id, Action<TEntity> update);
        void Upsert(TEntity entity);
        TEntity GetById(string id);
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate, int skip = 0 , int limit = int.MaxValue);
        void DeleteById(string id);
    }
}
