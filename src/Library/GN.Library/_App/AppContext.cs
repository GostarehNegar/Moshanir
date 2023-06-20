using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using GN.Library;
using GN.Library.Data;
using GN.Library.Transactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GN
{
	public interface IAppContextValues
	{
		bool TryGetValue<T>(out T value, string key = null);
		T GetOrAddValue<T>(Func<IAppContext, T> factory, string key = null);
		T AddOrUpdate<T>(Func<IAppContext, T> factory, string key = null);
		T GetValue<T>(string key = null);
		void RemoveValue<T>(string key = null);
	}
	public interface IAppContext : IDisposable
	{
		int Id { get; }
		IAppContextValues Values { get; }
		IAppServices AppServices { get; }
		//IAppDataServices DataServices { get; }
		//IAppUtils Utils { get; }
		//IAppConfiguration AppConfigurations { get; }
		IAppContext Push();
		IAppContext Parent { get; }
		IServiceScope Scope { get; }
		T GetService<T>();
		IServiceProvider ServiceProvider { get; }
		ITransactionContext TransactionContext { get; }
	}
	public class AppContext : IAppContext, IAppContextValues
	{
		private TransactionScopeContainer transactionScope;
		public int Id { get; private set; }
		private static AppContext root = new AppContext();
		internal static void Reset()
		{
			Root.serviceProvider = null;
			//root = new AppContext(); 
		}
		private IServiceProvider serviceProvider;
		private static AppContext current;
		public static AppContext Root => root;
		public static AppContext Current
		{
			get
			{
				current = current ?? root;
				if (current != null)
				{
				}
				return current;

			}
		}

		public IAppContextValues Values => this;
		public IAppUtils Utils => AppHost.Utils;
		public IAppServices AppServices { get; private set; }
		public IAppDataServices DataServices => GetService<IAppDataServices>();
		//public IAppConfiguration AppConfigurations => AppHost.Configuration;
		public IServiceProvider ServiceProvider
		{
			get
			{
				if (this.serviceProvider == null)
				{
					this.serviceProvider = this.parent?.ServiceProvider;
				}
				return this.serviceProvider;
			}
			private set
			{ this.serviceProvider = value; }
		}
		public AppContext parent { get; private set; }
		public IAppContext Parent => parent;
		public IServiceScope Scope { get; private set; }
		private AppContext()
		{
			this.headers = new ConcurrentDictionary<string, string>();
			this.cache = new ConcurrentDictionary<string, object>();
		}
		private bool IsDisposed()
		{
			return this.IsDisposed(this.serviceProvider);
		}
		private bool IsDisposed(IServiceProvider serviceProvider)
		{
			try
			{
				serviceProvider.GetServiceEx<ILoggerFactory>();
			}
			catch (ObjectDisposedException)
			{
				return true;
			}
			return false;
		}
		private AppContext(AppContext parent)
		{
			this.Scope = parent.ServiceProvider.CreateScope();
			this.parent = parent;
			this.ServiceProvider = this.Scope.ServiceProvider;
			if (parent != null)
			{
				//parent.RebuildHeaders();
				this.headers = new ConcurrentDictionary<string, string>(parent.RebuildHeaders());
				this.cache = new ConcurrentDictionary<string, object>();
			}
			if (IsDisposed(this.serviceProvider))
			{
				this.Scope = Root.serviceProvider.CreateScope();
				this.parent = Root;
				this.ServiceProvider = this.Scope.ServiceProvider;

			}
			init();
		}
		private void init(IServiceProvider sp=null)
		{
			this.Id = new Random().Next();
			if (serviceProvider != null)
				this.serviceProvider = sp;
			//this.Utils = new AppUtils(this);
			this.AppServices = new AppServices(this);
			//this.AppConfigurations = new AppConfiguration(this);

		}

		public IAppContext Push()
		{
			current = new AppContext(this);
			return current;
		}
		public static void Initialzie(IServiceProvider provider)
		{

			if (1==0 || Root.serviceProvider == null)
			{
				Root.ServiceProvider = provider;
				//AppContext.Reset();
				Root.init(provider);
			}
		}
		public static bool IsInitailzied => root.ServiceProvider != null;

		//public IAppDataServices DataServices => throw new NotImplementedException();
		public ITransactionContext TransactionContext => this.GetTransactionScope().TransactionContext;
		public TransactionScopeContainer TranscationScope => this.GetTransactionScope();
		private TransactionScopeContainer GetTransactionScope()
		{
			if (this.transactionScope == null)
			{
				this.transactionScope = new TransactionScopeContainer(this);
			}
			return this.transactionScope;
		}

		#region  Cloneable Dictionary

		private ConcurrentDictionary<string, string> headers = new ConcurrentDictionary<string, string>();
		private ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();
		private static string GetKey(Type type, string key)
		{
			return string.Format("{0}|{1}", type?.FullName, key);
		}
		internal static bool _IsNullable(Type type)
		{
			return type == null || !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
		}
		private IEnumerable<KeyValuePair<string, string>> RebuildHeaders()
		{
			foreach (var item in this.headers)
			{
				if (this.cache.TryGetValue(item.Key, out var tmp))
				{
					this.headers[item.Key] = Serialize(tmp);
				}
			}
			return this.headers.ToArray();

		}
		public bool TryGetValue<T>(out T value, string key = null)
		{
			var _key = GetKey(typeof(T), key);
			if (this.cache.TryGetValue(_key, out var tmp))
			{
				if (tmp != null && typeof(T).IsAssignableFrom(tmp.GetType()))
				{
					value = (T)tmp;
					return true;
				}
				else if (tmp == null && _IsNullable(typeof(T)))
				{
					value = (T)tmp;
					return true;
				}

			}
			if (this.headers.TryGetValue(_key, out var _tmp) && _tmp != null)
			{
				try
				{
					var _value = Deserialize<T>(_tmp);//  Newtonsoft.Json.JsonConvert.DeserializeObject<T>(_tmp);
					this.cache.AddOrUpdate(_key, _value, (a, b) => _value);
					value = _value;
					return true;
				}
				catch { }
			}
			value = default(T);
			return false;
		}
		public T GetOrAddValue<T>(Func<IAppContext, T> factory, string key = null)
		{
			T result = default(T);
			if (TryGetValue<T>(out result, key))
				return result;
			return AddOrUpdate<T>(factory, key);
			//if (factory != null)
			//	result = factory(this);
			//if (result != null)
			//{
			//	AddOrUpdate<T>(result, key);
			//}
			//return result
		}
		public T AddOrUpdate<T>(Func<IAppContext, T> factory, string key = null)
		{
			var _key = GetKey(typeof(T), key);
			T value = factory == null
				? default(T)
				: factory(this);
			if (value != null)
			{
				this.cache.AddOrUpdate(_key, value, (a, b) => value);
				var str_value = Serialize<T>(value);
				this.headers.AddOrUpdate(_key, str_value, (a, b) => str_value);
			}
			return value;
		}
		public T GetValue<T>(string key = null)
		{
			return TryGetValue<T>(out var tmp, key)
				? tmp
				: default;
		}
		public void RemoveValue<T>(string key)
		{
			var _key = GetKey(typeof(T), key);
			this.cache.TryRemove(_key, out var _);
			this.headers.TryRemove(_key, out var _);
		}
		private string Serialize<T>(T value)
		{
			if (value == null)
				return null;
			return value.GetType().IsAbstract
				? $"{value.GetType().AssemblyQualifiedName}%{JsonConvert.SerializeObject(value)}"
				: $"{value.GetType().AssemblyQualifiedName}%{JsonConvert.SerializeObject(value)}";

		}
		private T Deserialize<T>(string value)
		{
			if (string.IsNullOrEmpty(value))
				return default(T);
			var parts = value.Split(new char[] { '%' }, 2, StringSplitOptions.None);
			var json = parts.Length < 2 ? parts[0] : parts[1];
			try
			{
				return JsonConvert.DeserializeObject<T>(json);
			}
			catch { }
			try
			{
				return (T)JsonConvert.DeserializeObject(json, Type.GetType(parts[0]));
			}
			catch { }
			return default(T);
		}
		#endregion
		public void Dispose()
		{
			this.transactionScope?.Dispose();
			this.Scope?.Dispose();
			this.Scope = null;
			current = this.parent == null || this.parent.IsDisposed() ? Root : this.parent;
		}

		public T GetService<T>()
		{
			return this.AppServices.GetService<T>();
		}
	}
}
