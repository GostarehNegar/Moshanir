using GN.Library.Data.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Data
{
	public interface IUserDocumentStore : ILiteDbContext
	{
	}
	public interface IUserDocumentStoreRepository<TId, TEntity> : IDocumentRepository<TId, TEntity> where TEntity : class
	{

	}
}
