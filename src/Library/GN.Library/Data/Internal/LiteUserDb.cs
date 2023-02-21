using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Data.Internal
{
	class UserDbConfig : LiteDbConfiguration
	{
		public UserDbConfig(LibOptions options)
		{
			this.ConnectionString = options.GetUserDbFileName();
		}
	}
	class UserDb : LiteDbContext<UserDbConfig>, IUserDocumentStore
	{
		public UserDb(UserDbConfig config) : base(config) { }
	}
	class UserDbRepository<TId, TEntity> : DocumentRepository<IUserDocumentStore, TId, TEntity>, IUserDocumentStoreRepository<TId, TEntity> where TEntity : class
	{
		public UserDbRepository(IUserDocumentStore context) : base(context)
		{

		}
	}
}
