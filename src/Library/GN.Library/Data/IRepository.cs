using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Data
{
	public interface IRepository : IDisposable
	{
	}
	public interface IRepository<TId, TEntity> : IRepository, IDbSet<TId, TEntity> where TEntity : class
	{
		Task<IRepository<TId, TEntity>> Lock(int timeOut = 10 * 1000, CancellationToken cancellationToken = default);
	}
	public class Repository<TDbContext, TId, TEntity> : IRepository<TId, TEntity>
		where TEntity : class
		where TDbContext : IDbContext
	{
		public TDbContext DbContext { get; protected set; }
		public IDbSet<TId, TEntity> DbSet { get; protected set; }
		public Repository(TDbContext context)
		{

			this.DbContext = context;
			if (context != null)
				this.DbSet = context.GetDbSet<TId, TEntity>();
		}
		public TId Insert(TEntity item)
		{
			return this.DbSet.Insert(item);
		}

		public void Delete(TId id)
		{
			this.DbSet.Delete(id);
		}
		public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> predicate)
		{
			return this.DbSet.Get(predicate);
		}


		public virtual TEntity Get(TId id)
		{
			return this.DbSet.Get(id);
		}

		public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> predicate, int skip = 0, int count = 50000)
		{
			return this.DbSet.Get(predicate, skip, count);
		}

		public long Count(Expression<Func<TEntity, bool>> predicate)
		{
			return this.DbSet.Count(predicate);
		}

		public void EnsureIndex<K>(Expression<Func<TEntity, K>> property, bool isUnique = false)
		{
			this.DbSet.EnsureIndex<K>(property, isUnique);
		}

		public void Update(TEntity value)
		{
			this.DbSet.Update(value);
		}

		public void Dispose()
		{

		}

		public TEntity Upsert(TEntity item)
		{
			return this.DbSet.Upsert(item);
		}

		public Task<IRepository<TId, TEntity>> Lock(int timeOut = 10 * 1000, CancellationToken cancellationToken = default)
		{
			return Task.FromResult<IRepository<TId, TEntity>>(this);
		}
	}


}
