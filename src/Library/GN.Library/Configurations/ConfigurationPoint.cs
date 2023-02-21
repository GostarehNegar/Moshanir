using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Configurations
{
	class AppConfigurationPoint
	{
		public bool Created { get; private set; }
		public object Value { get; private set; }
		public string Key { get; private set; }
		public Type Type { get; private set; }
		public Func<ConfigurationFactoryContext, object> Factory { get; set; }
		public AppConfigurationPoint(Type type, string key, Func<ConfigurationFactoryContext, object> factory)
		{
            this.Type = type;
			this.Key = key;
			this.Factory = factory;
		}
		public AppConfigurationPoint Clone()
		{
			var result = new AppConfigurationPoint(this.Type, this.Key, this.Factory)
			{
				Value = this.Value
			};
			return result;

		}
		public void Update(string key, Func<ConfigurationFactoryContext, object> factory)
		{
			this.Factory = factory ?? this.Factory;
			this.Key = key ?? this.Key;
			this.Created = false;
		}
		internal bool TryGetValue<T>(ConfigurationFactoryContext ctx, out T result, bool refresh = false)
		{
			result = default(T);
			if (!Created || refresh)
			{
				this.Value = this.Factory?.Invoke(ctx);
                if (this.Value != null)
                    this.Type = this.Value.GetType();
				this.Created = true;
			}
			if (this.Value != null && typeof(T).IsAssignableFrom(this.Value.GetType()))
			{
				result = (T)this.Value;
				return true;
			}
			return false;
		}
	}
}
