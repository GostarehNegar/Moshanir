using GN.Library.Data.Internal;
using LiteDB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GN.Library.Data.Deprecated
{
	internal interface IDocumentStoreFactory
	{
		T CreateDb<T>(string connectionString);
		LiteDB.LiteDatabase Create();
	}

	internal class DocumentStoreFactorr : IDocumentStoreFactory
	{
		public LiteDB.LiteDatabase Create()
		{
			throw new NotImplementedException();
		}

		public T CreateDb<T>(string connectionString)
		{
			throw new NotImplementedException();
		}

		public void Test()
		{
			var db = new LiteDB.LiteDatabase("");
			var col = db.GetCollection<string>();

		}
	}
	public interface IDocummentStore_Deprecated : IDisposable
	{
		IDocummentStore_Deprecated init(string connectionString);
		IDocumentRepository_Deprecated<T> GetRepository_deprecated<T>();
		//IDocumentRepositoryDeprecated<T> GetRepository<T>();
		LiteDatabase GetDatabase();
	}
	public class DocumentStore : IDocummentStore_Deprecated
	{
		protected LiteDatabase db;
		protected ConcurrentDictionary<Type, object> repositories;
		protected DocumentStoreConnectionString connectionString;

		public DocumentStore(string connectionString = "")
		{
			init(connectionString);
		}

		public IDocummentStore_Deprecated init(string connectionString)
		{
			this.repositories = new ConcurrentDictionary<Type, object>();
			this.connectionString = new DocumentStoreConnectionString(connectionString);
			return this;
		}
		public LiteDatabase GetDatabase()
		{
			if (this.db == null)
			{
				if (!Directory.Exists(Path.GetDirectoryName(this.connectionString.FileName)))
					Directory.CreateDirectory(Path.GetDirectoryName(this.connectionString.FileName));
				this.db = new LiteDatabase(this.connectionString.ConnectionString);
			}
			return this.db;
		}

		public IDocumentRepository_Deprecated<T> GetRepository_deprecated<T>()
		{
			var type = typeof(T);
			var result = this.repositories.GetOrAdd(type, x =>
			{
				return AppHost
				.GetService<IDocumentRepository_Deprecated<T>>()
				.init(this.GetDatabase(), this.connectionString.ToString());
				//return new DocumentRepository<T>(this.GetDatabase());
			});
			return result as IDocumentRepository_Deprecated<T>;
		}

		public virtual bool IsDisposable()
		{
			return true;
		}
		public void Dispose()
		{
			if (IsDisposable())
				this.db?.Dispose();
		}
	}

}
