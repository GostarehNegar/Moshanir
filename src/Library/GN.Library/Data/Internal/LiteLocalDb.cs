using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Data.Internal
{
    class LocalDbConfig : LiteDbConfiguration
    {
        public LocalDbConfig(LibOptions options)
        {
            this.ConnectionString = options.GetLocalDbFileName();
        }
    }
    class LocalDb : LiteDbContext<LocalDbConfig>, ILocalDocumentStore
    {
        public LocalDb(LocalDbConfig config) : base(config) { }
    }
    class LocalDbRepository<TId, TEntity> : DocumentRepository<ILocalDocumentStore, TId, TEntity>, ILocalDocumentStoreRepository<TId, TEntity> where TEntity : class
    {
        public LocalDbRepository(ILocalDocumentStore context) : base(context)
        {

        }
    }

}
