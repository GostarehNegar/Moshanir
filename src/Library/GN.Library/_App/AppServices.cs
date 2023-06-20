using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace GN.Library
{
	
	public interface IAppServices : IServiceProvider
	{
		T GetService<T>();
		IServiceProvider Provider { get; }
		//IAppObjectFactory Factory { get; }
		IAppUtils Utils { get; }
		IAppMapper Mapper { get; }
		IAppDataServices DataServices { get; }

	}

	internal class AppServices : Data.AppContextBase, IAppServices
	{
		private IServiceProvider provider;
		public IServiceProvider Provider => this.provider;
		//public IAppObjectFactory Factory { get; private set; }
		private ManualResetEventSlim _lock = new ManualResetEventSlim(true);
		public IAppUtils Utils => new AppUtils();
		public IAppDataServices DataServices => GetService<IAppDataServices>();

		public IAppMapper Mapper => this.GetService<IAppMapper>();

		public AppServices(IAppContext context) : base(context)
		{
			//this.Factory = new AppObjectFactory(this);
			this.provider = context.ServiceProvider;
		}
		public object GetService(Type serviceType)
		{
			return this.provider.GetService(serviceType);
		}
		public T GetService<T>()
		{
			return this.provider.GetServiceEx<T>();
		}
		public void SetProvider(IServiceProvider provider)
		{
			this.provider = provider;
		}
		public void Lock()
		{
			_lock.Set();
		}
		public void UnLock()
		{
			_lock.Reset();
		}


	}
}
