using GN.Library.Data.Deprecated;
using GN.Library.Data.Internal;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Data
{
	public interface IDocumentRepository<TKey, T> : IRepository<TKey, T> where T : class
	{
		IEnumerable<T> Find(Expression<Func<T, bool>> filter, int skip = 0, int count = 2000000000);
		bool Any(Expression<Func<T, bool>> predicate);
		bool Exists(TKey id);
		//T Upsert(T item);
		IDocumentRepository<TKey, T> init(object owner = null, string connectionString = null);

	}
	public interface IDocumentRepository<TEntity> : IDocumentRepository<object, TEntity> where TEntity : class { }
	public class DocumentRepository<TContext, TKey, T> : IDocumentRepository<TKey, T>
		where T : class
		where TContext : ILiteDbContext
	{
		public static string DB_DIR = "./Data";
		public static string DB_FILENAME = "db";
		private LiteDatabase database;
		private ILiteDbContext OwnerAsContext { get { return this.owner as ILiteDbContext; } }
		private LiteDatabase OwnerAsDatabase { get { return this.owner as LiteDatabase; } }
		private object owner;

		private ILiteCollection<T> collection;
		//protected string connectionString;
		public DocumentStoreConnectionString connectionString;
		public string ConnectionString => this.connectionString?.ToString();
		protected bool IsDbOwner;
		public DocumentRepository(string connectionString = null)
		{
			this.connectionString = new DocumentStoreConnectionString(connectionString);
		}
		public DocumentRepository(LiteDatabase db)
		{
			this.database = db;
			this.IsDbOwner = false;
		}
		public DocumentRepository(TContext owner)
		{
			this.owner = owner;
		}
		//public DocumentRepository(IDocummentStore_Deprecated store) : this(store.GetDatabase())
		//{

		//}
		public IDocumentRepository<TKey, T> init(object owner = null, string connectionString = null)
		{
			this.connectionString = new DocumentStoreConnectionString(connectionString);
			this.owner = owner;
			return this;
		}
		protected ILiteCollection<T> Collection => GetCollection();

		protected virtual string GetConnectionString()
		{
			return this.connectionString.ConnectionString;
		}

		protected LiteDatabase GetDatabase(bool refresh)
		{
			if (this.database == null || refresh)
			{
				if (this.OwnerAsDatabase != null)
				{
					this.IsDbOwner = false;
					this.database = this.OwnerAsDatabase;
				}
				else if (this.OwnerAsContext != null)
				{
					if (1 == 1)
					{
						this.database = this.OwnerAsContext.CreateDatabase();
						this.IsDbOwner = true;
					}
					else
					{
						this.database = this.OwnerAsContext.GetDatabase();
						this.IsDbOwner = false;
						this.OwnerAsContext.OnDatabaseDisposed += (a, b) =>
						{
							this.database = null;

						};
					}
				}
				else
				{
					var fileName = this.connectionString?.FileName;
					if (!string.IsNullOrWhiteSpace(fileName))
					{
						try
						{
							if (!Directory.Exists(Path.GetDirectoryName(fileName)))
								Directory.CreateDirectory(Path.GetDirectoryName(fileName));
						}
						catch { }
					}

					this.database = new LiteDatabase(GetConnectionString());
					this.IsDbOwner = true;
				}
				if (this.database == null)
				{
					throw new Exception(string.Format(
						"Faild to create database: '{0}' ", this.connectionString?.ToString()));

				}
			}
			return this.database;
		}
		protected virtual void OnCollectionCreated(ILiteCollection<T> collection)
		{

		}
		protected virtual void OnCollectionCreated()
		{

		}

		protected virtual ILiteCollection<T> GetCollection(bool refresh = false)
		{
			if (this.collection == null || refresh)
			{
				this.collection = this.GetDatabase(refresh).GetCollection<T>();
				OnCollectionCreated(this.collection);
				OnCollectionCreated();
			}
			return this.collection;

		}
		public virtual void Delete(TKey id)
		{
			var f = this.Collection.Delete(new BsonValue(id));

		}


		public void Dispose()
		{
			if (this.IsDbOwner)
				this.database?.Dispose();
		}

		public void EnsureIndex<K>(Expression<Func<T, K>> property, bool isUnique = false)
		{
			this.Collection.EnsureIndex<K>(property, isUnique);
		}

		public IEnumerable<T> Find(Expression<Func<T, bool>> filter, int skip = 0, int count = 2000000000)
		{
			return this.Collection.Find(filter, skip, count);
		}
		public IEnumerable<T> Get(Expression<Func<T, bool>> filter, int skip = 0, int count = 2000000000)
		{
			return filter == null
			? this.Collection.Find(x => true, skip, count)
			: this.Collection.Find(filter, skip, count);
		}
		public virtual T Get(TKey id)
		{
			return this.Collection.FindById(new BsonValue(id));
		}

		public IEnumerable<T> GetAll()
		{
			return this.Collection.FindAll();
		}

		public long GetCount()
		{
			return this.Collection.LongCount();
		}

		public T GetFirstOrDefault(Expression<Func<T, bool>> filter)
		{
			return this.Collection.FindOne(filter);
		}

		public TKey Insert(T item)
		{

			var ret = this.Collection.Insert(item);
			return (TKey)ret.RawValue;
		}
		public virtual void Update(T item)
		{

		}
		public virtual T Upsert(T item)
		{
			this.Collection.Upsert(item);
			return item;
		}

		public bool Any(Expression<Func<T, bool>> predicate)
		{
			return this.Collection.Exists(predicate);
		}

		public void Delete(Expression<Func<T, bool>> predicate)
		{
			//this.Collection.Delete(predicate);
			this.Collection.DeleteMany(predicate);

		}

		public long Count(Expression<Func<T, bool>> predicate)
		{
			return predicate == null
			? this.Collection.LongCount()
			: this.Collection.LongCount(predicate);
		}

		public long LongCount(Expression<Func<T, bool>> predicate)
		{
			return this.Collection.LongCount(predicate);
		}

		public long LongCount()
		{
			return this.Collection.LongCount();
		}

		public bool Exists(TKey id)
		{
			return this.GetCollection().Exists(Query.EQ("_id", new BsonValue(id)));
		}



		public async Task<IRepository<TKey, T>> Lock(int timeOut = 10 * 1000, CancellationToken cancellationToken = default)
		{
			var start = DateTime.UtcNow;
			while (this.database == null && (DateTime.UtcNow - start).TotalMilliseconds < timeOut && !cancellationToken.IsCancellationRequested)
			{
				try
				{
					if (this.GetDatabase(false) != null)
						break;
					await Task.Delay(100);
				}
				catch (Exception err)
				{
				}
			}
			_ = this.GetDatabase(false);
			return this;
		}
	}

	public class DocumentRepository<TKey, T> : DocumentRepository<ILiteDbContext, TKey, T>, IDocumentRepository<TKey, T>
		where T : class
	{
		public DocumentRepository(string connectionString = null) : base(connectionString)
		{

		}
		public DocumentRepository(LiteDatabase db) : base(db)
		{
		}
		public DocumentRepository(ILiteDbContext owner) : base(owner)
		{

		}

	}
	public class DocumentRepository<TEntity> : DocumentRepository<object, TEntity> where TEntity : class
	{
		public DocumentRepository(string connectionString = null) : base(connectionString)
		{

		}
		public DocumentRepository(ILiteDbContext context) : base(context)
		{

		}
		public DocumentRepository(LiteDatabase db) : base(db)
		{

		}
	}
}

