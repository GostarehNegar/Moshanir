using LiteDB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GN.Library.Data.Internal
{

	class LiteDbConfiguration : IDbContextConfiguration
	{
		public IReadOnlyDictionary<string, string> Properties { get; } = new Dictionary<string, string>();
		public string ConnectionString { get; set; }
	}
	public interface ILiteDbContext : IDbContext
	{
		LiteDatabase GetDatabase();
		event EventHandler OnDatabaseDisposed;
		LiteDatabase CreateDatabase();
	}
	class LiteDbContext<TConfig> : ILiteDbContext, IDbContext<TConfig> where TConfig : LiteDbConfiguration
	{
		public TConfig Configuration { get; private set; }
		private LiteDatabase db;
		protected ConcurrentDictionary<Type, object> repositories = new ConcurrentDictionary<Type, object>();
		public event EventHandler OnDatabaseDisposed;
		public LiteDbContext(TConfig configuration)
		{
			this.Configuration = configuration;
		}
		public LiteDatabase CreateDatabase()
		{
			return new LiteDatabase(this.Configuration.ConnectionString);
		}

		public LiteDatabase GetDatabase()
		{
			if (this.db == null)
			{
				this.db = new LiteDatabase(this.Configuration.ConnectionString);
			}
			return this.db;
		}

		public IDbSet<TId, TEntity> GetDbSet<TId, TEntity>() where TEntity : class
		{
			return new LiteDbDbSet<TId, TEntity>(this);
		}
		public void Dispose()
		{
			this.repositories.Values
				.Select(x => x as IRepository)
				.Where(x => x != null)
				.ToList()
				.ForEach(x => x?.Dispose());
			this.db?.Dispose();
			this.db = null;
			if (this.OnDatabaseDisposed != null)
				OnDatabaseDisposed(this, new EventArgs { });

		}
		public IRepository<TId, TEntity> GetRepository<TId, TEntity>(bool cache = false, Func<IDbContext, IRepository<TId, TEntity>> factory = null) where TEntity : class
		{
			IRepository<TId, TEntity> result = null;
			IRepository<TId, TEntity> _factory()
			{
				return factory?.Invoke(this) ?? AppHost.Services.GetService<IRepository<TId, TEntity>>() ?? new DocumentRepository<TId, TEntity>(this);

			}
			if (cache)
			{
				var _result = this.repositories.GetOrAdd(typeof(IRepository<TId, TEntity>), k => _factory());
				result = _result as IRepository<TId, TEntity>;
			}
			else
			{
				result = _factory();
			}
			return result;
		}

		public T GetRepository<T>(bool cache = false, Func<IDbContext, T> factory = null) where T : class, IRepository
		{
			T result = null;
			T _factory()
			{
				return factory?.Invoke(this) ?? AppHost.Services.GetService<T>();

			}
			if (cache)
			{
				var _result = this.repositories.GetOrAdd(typeof(T), k => _factory());
				result = _result as T;
			}
			else
			{
				result = _factory();
			}
			return result;
		}
	}

}
