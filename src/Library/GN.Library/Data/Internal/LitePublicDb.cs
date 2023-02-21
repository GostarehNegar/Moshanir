using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Data.Internal
{
	class PublicDbConfig : LiteDbConfiguration
	{
		public PublicDbConfig(LibOptions options)
		{
			this.ConnectionString = options.GetPublicDbFileName();
		}
	}
	class PublicDb : LiteDbContext<PublicDbConfig>, IPublicDocumentStore
	{
		public PublicDb(PublicDbConfig config) : base(config) { }
	}
	class PublicDbRepository<TId, TEntity> : DocumentRepository<IPublicDocumentStore, TId, TEntity>, IPublicDbRepository<TId, TEntity> where TEntity : class
	{
		public PublicDbRepository(IPublicDocumentStore context) : base(context)
		{

		}
	}
}
