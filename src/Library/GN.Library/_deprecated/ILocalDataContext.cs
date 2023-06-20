using GN.Library.Data.Deprecated;
using GN.Library.Data.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Data
{
	public interface ILocalDataContext : IDocummentStore_Deprecated
	{
	}
	public interface IUserDataContext : IDocummentStore_Deprecated
	{
	}
	public interface IPublicDataContext : IDocummentStore_Deprecated
	{
	}
	public interface IGlobalDataContext : IDocummentStore_Deprecated
	{
	}


	class LocalDataContext : DocumentStore, ILocalDataContext
	{
		public LocalDataContext()
		{
			this.connectionString = new DocumentStoreConnectionString()
			{
				FileName = LibOptions.Current.GetLocalDbFileName()
			};
		}
	}
	class UserDataContext : DocumentStore, IUserDataContext
	{
		public UserDataContext()
		{
			this.connectionString = new DocumentStoreConnectionString()
			{
				FileName = LibOptions.Current.GetUserDbFileName()
			};
		}
	}
	class PublicDataContext : DocumentStore, IPublicDataContext
	{
		public PublicDataContext()
		{
			this.connectionString = new DocumentStoreConnectionString()
			{
				FileName = LibOptions.Current.GetPublicDbFileName()
			};
		}
	}
	class GlobalDataContext : DocumentStore, IGlobalDataContext
	{
		public GlobalDataContext()
		{
			this.connectionString = new DocumentStoreConnectionString()
			{
				FileName = LibOptions.Current.GetGlobalDbFileName()
			};
		}
	}


}
