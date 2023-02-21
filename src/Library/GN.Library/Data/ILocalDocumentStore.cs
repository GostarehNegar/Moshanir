using GN.Library.Data.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Data
{
	public interface ILocalDocumentStore : ILiteDbContext
	{

	}
	public interface ILocalDocumentStoreRepository<TId, TEntity> : IDocumentRepository<TId, TEntity> where TEntity : class
	{

	}
}
