using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GN.Library;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GN.Library
{
	public interface IAppContext_Deprecated : IDisposable
	{
		IAppServices AppServices { get; }
		IAppContext_Deprecated Parent { get; }
		IAppContext_Deprecated GetRoot();
		IAppContext_Deprecated Push();
		IAppConfiguration Configuration { get; }
		IAppUtils Utils { get; }
		IAppDataServices DataContext { get; }
		T GetService<T>();
	}

	class AppContext_Deprecated : IAppContext_Deprecated
	{
		//private readonly IAppUtils utils = new AppUtils();
		private IServiceScope scope;
		private AppServices appServices;
		private static IAppContext_Deprecated root;
		private static IAppContext_Deprecated current;

		public static IAppContext_Deprecated Current
		{
			get
			{
				if (root == null)
					throw new Exception("Invalid Context. It seems that system is not initialized properly! ");
				return current ?? root;
			}
		}
		public static void Initialize(IServiceProvider provider)
		{
			if (root == null)
				root = new AppContext_Deprecated(null, provider);
		}
		public static bool Initialized => root != null;
		public static void Reset()
		{
			root = null;
			current = null;
		}

		public AppContext_Deprecated(IAppContext_Deprecated parent, IServiceProvider provider)
		{
			if (parent == null && provider == null)
			{
				throw new ArgumentException("'SeviceProvider' and 'parent' cannot be both NULL.");
			}
			if (parent == null)
			{
				appServices = new AppServices(AppHost.Context);
				this.Configuration = AppStartup_Deprecated.AppConfiguration;
			}
			else
			{
				this.Parent = parent;
				this.scope = parent.AppServices.CreateScope();
				this.appServices = new AppServices(AppHost.Context);
				this.Configuration = new AppConfiguration(this, appServices.GetService<IConfiguration>());
			}

		}
		public IAppContext_Deprecated Push()
		{
			IAppContext_Deprecated result = new AppContext_Deprecated(this, null);
			current = result;
			return result;
		}
		public IAppContext_Deprecated Pop(bool disposing)
		{
			if (this.Parent != null)
			{
				current = this.Parent;
				if (disposing)
				{
					/// Lock appservices to prevent
					/// Disposed Poviders Exception. 
					this.appServices.Lock();
					this.appServices.SetProvider(this.Parent.AppServices.Provider);
					this.appServices.UnLock();
					this.scope?.Dispose();
				}
			}
			return current;
		}
		public IAppContext_Deprecated GetRoot()
		{
			return this.Parent == null ? this : this.Parent.GetRoot();
		}
		public T GetService<T>()
		{
			return this.AppServices.GetService<T>();
		}

		public Guid Id = Guid.NewGuid();
		public IAppUtils Utils => new AppUtils(AppHost.Context);
		public IAppContext_Deprecated Parent { get; private set; }
		public IAppConfiguration Configuration { get; protected set; }
		public IAppServices AppServices => appServices;
		public IAppDataServices DataContext => AppServices.GetService<IAppDataServices>();
		public void Dispose(bool disposing)
		{

			if (disposing)
			{
				Pop(disposing);
			}
		}
		public void Dispose()
		{
			Dispose(true);
		}
		~AppContext_Deprecated()
		{
			Dispose(false);
		}
	}


}
