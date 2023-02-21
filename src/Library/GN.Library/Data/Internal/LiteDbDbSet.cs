using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace GN.Library.Data.Internal
{
	class LiteDbDbSet<TId, TEntity> : IDbSet<TId, TEntity> where TEntity : class
	{

		protected ILiteDbContext context;
		private LiteDB.ILiteCollection<TEntity> collection;
		private Func<LiteDatabase> factory;
		public LiteDbDbSet(ILiteDbContext context)
		{
			this.context = context;
			this.context.OnDatabaseDisposed += (s, e) =>
			{
				this.collection = null;
			};
		}
		protected LiteDatabase GetDatabase()
		{
			return this.context.GetDatabase();
		}
		protected ILiteCollection<TEntity> GetCollection()
		{
			if (this.collection == null)
			{
				this.collection = this.context.GetDatabase().GetCollection<TEntity>();
			}
			return this.collection;
		}

		public long Count(Expression<Func<TEntity, bool>> predicate)
		{
			return GetCollection().LongCount(predicate);
		}

		public void Delete(TId id)
		{
			GetCollection().Delete(new BsonValue(id));
		}

		public void EnsureIndex<K>(Expression<Func<TEntity, K>> property, bool isUnique = false)
		{
			this.GetCollection().EnsureIndex(property, isUnique);
		}

		public TEntity Get(TId id)
		{
			return this.GetCollection().FindById(new BsonValue(id));
		}

		public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> predicate, int skip = 0, int count = 50000)
		{
			return this.GetCollection().Find(predicate, skip, count);
		}

		public TId Insert(TEntity item)
		{
			var result = this.GetCollection().Insert(item);
			return (TId)result.RawValue;
		}

		public void Update(TEntity value)
		{
			
			this.GetCollection().Update(value);
		}

		public void Dispose()
		{
			
		}

		public TEntity Upsert(TEntity item)
		{
			throw new NotImplementedException();
		}
	}
}
