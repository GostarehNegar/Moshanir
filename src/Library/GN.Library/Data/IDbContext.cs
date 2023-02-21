using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace GN.Library.Data
{
	public interface IDbContextConfiguration
	{
		IReadOnlyDictionary<string, string> Properties { get; }
		string ConnectionString { get; }
	}
	public interface IDbSet<TId, TEntity> : IDisposable where TEntity : class
	{
		TId Insert(TEntity item);
		void Delete(TId id);
		void Update(TEntity value);
		TEntity Get(TId id);
		TEntity Upsert(TEntity item);
		IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> predicate, int skip = 0, int count = 50000);
		long Count(Expression<Func<TEntity, bool>> predicate = null);
		void EnsureIndex<K>(Expression<Func<TEntity, K>> property, bool isUnique = false);
	}
	public interface IDbContext : IDisposable
	{
		IDbSet<TId, TEntity> GetDbSet<TId, TEntity>() where TEntity : class;
		IRepository<TId, TEntity> GetRepository<TId, TEntity>(bool cache = false, Func<IDbContext, IRepository<TId, TEntity>> factory = null) where TEntity : class;
		T GetRepository<T>(bool cache = false, Func<IDbContext, T> factory = null) where T : class, IRepository;

	}
	public interface IDbContext<TConfiguration> : IDbContext where TConfiguration : IDbContextConfiguration
	{
		TConfiguration Configuration { get; }
	}
	public interface IDataContext
	{
		IDbContext DbContext { get; }
	}
	public interface IDataContext<TDbContext> : IDataContext where TDbContext : IDbContext
	{
		new TDbContext DbContext { get; }
	}

}
