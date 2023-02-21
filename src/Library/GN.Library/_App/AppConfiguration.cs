
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GN.Library;
using GN.CodeGuard;
using GN.Library.Configurations;
using GN.Library.Collections;


namespace GN.Library
{



	public interface IAppConfiguration
	{

		AppBuildOptions Options { get; }
		IConfiguration Configuration { get; }
		T GetOrAddValue<T>(string key = null, Func<IAppConfiguration, T> factory = null);
		T Update<T>(string key, Func<IAppConfiguration, T> factory);
		void Save();
	}

	class AppConfiguration : IAppConfiguration
	{

		CloneableDictionary Values = new CloneableDictionary();
		public AppBuildOptions Options => AppBuildOptions.Current;
		public IConfiguration Configuration { get; private set; }


		public AppConfiguration(IConfiguration configuration)
		{
			this.Configuration = configuration;
		}
		public void Save()
		{
		}

		public T GetOrAddValue<T>(string key = null, Func<IAppConfiguration, T> factory = null)
		{
			return this.Values.GetOrAddValue<T>(() =>
			{
				return factory == null ? default(T) : factory(this);
			}, key);
		}

		public T Update<T>(string key, Func<IAppConfiguration, T> factory)
		{
			var result = factory == null ? default(T) : factory(this);
			if (result != null)
				this.Values.AddOrUpdate(result, key);
			return result;
		}
	}


}
