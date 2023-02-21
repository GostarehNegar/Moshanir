using GN.Library.Data.Internal;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace GN.Library.Data.Deprecated
{
	public interface IDocumentRepository_Deprecated<T> : IDisposable
	{
		T Insert(T item);
		T Upsert(T item);
		IDocumentRepository_Deprecated<T> EnsureIndex<K>(Expression<Func<T, K>> property, bool isUnique = false);
		void Delete(object id);
		IEnumerable<T> Find(Expression<Func<T, bool>> filter, int skip = 0, int count = 2000000);
		T GetFirstOrDefault(Expression<Func<T, bool>> filter);
		T Get(object id);
		long GetCount();
		IEnumerable<T> GetAll();
		bool Any(Expression<Func<T, bool>> predicate);
		void Delete(Expression<Func<T, bool>> predicate);
		long LongCount(Expression<Func<T, bool>> predicate);
		long LongCount();
		IDocumentRepository_Deprecated<T> init(LiteDatabase db = null, string connectionString = null);
		string ConnectionString { get; }
	}
	public class DocumentRepository_Deprecated<T> : IDocumentRepository_Deprecated<T>
	{
		public static string DB_DIR = "./Data";
		public static string DB_FILENAME = "db";
		private LiteDatabase database;
		private ILiteCollection<T> collection;
		//protected string connectionString;
		public DocumentStoreConnectionString connectionString;
		public string ConnectionString => this.connectionString?.ToString();
		protected bool IsDbOwner;
		public DocumentRepository_Deprecated(string connectionString = null)
		{
			this.connectionString = new DocumentStoreConnectionString(connectionString);
		}
		public DocumentRepository_Deprecated(LiteDatabase db)
		{
			this.database = db;
			this.IsDbOwner = false;
		}
		public DocumentRepository_Deprecated(IDocummentStore_Deprecated store) : this(store.GetDatabase())
		{

		}
		public IDocumentRepository_Deprecated<T> init(LiteDatabase db = null, string connectionString = null)
		{
			this.connectionString = new DocumentStoreConnectionString(connectionString);
			if (db != null)
			{
				this.database = db;
				this.IsDbOwner = false;
			}
			else
			{
				//this.connectionString = new DocumentStoreConnectionString(connectionString);
				this.IsDbOwner = true;
			}
			return this;
		}
		protected ILiteCollection<T> Collection => GetCollection();

		protected virtual string GetConnectionString()
		{
			return this.connectionString.ConnectionString;
		}

		protected LiteDatabase GetDatabase(bool refresh)
		{
			if (this.database == null || refresh && this.IsDbOwner)
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
		public void Delete(object id)
		{
			var f = this.Collection.Delete(new BsonValue(id));
		}

		public void Dispose()
		{
			if (this.IsDbOwner)
				this.database?.Dispose();
		}

		public IDocumentRepository_Deprecated<T> EnsureIndex<K>(Expression<Func<T, K>> property, bool isUnique = false)
		{

			this.Collection.EnsureIndex<K>(property, isUnique);
			return this;
		}

		public IEnumerable<T> Find(Expression<Func<T, bool>> filter, int skip = 0, int count = 2000000000)
		{
			return this.Collection.Find(filter, skip, count);
		}

		public T Get(object id)
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

		public T Insert(T item)
		{

			var ret = this.Collection.Insert(item);
			return item;
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
			this.Collection.DeleteMany(predicate);

		}

		public long LongCount(Expression<Func<T, bool>> predicate)
		{
			return this.Collection.LongCount(predicate);
		}

		public long LongCount()
		{
			return this.Collection.LongCount();
		}
	}



}
