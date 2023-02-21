using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Configurations
{
	public class ConfigurationFactoryContext
	{
		private AppConfigurationPoint point;
		public IAppConfiguration Settings { get; private set; }
		public T GetCurrentValue<T>()
		{
			T result = default(T);
			if (point != null && point.Value != null)
			{
				result = point.Value == null || !typeof(T).IsAssignableFrom(point.Value.GetType())
					? default(T)
					: (T)point.Value;
			}
			else if (point != null && this.Settings != null && this.Settings.Parent != null)
			{
				result = this.Settings.Parent.GetOrAddValue<T>(point.Key, null);
			}
			return result;
		}

		internal ConfigurationFactoryContext(AppConfiguration settings, AppConfigurationPoint val)
		{
			this.Settings = settings;
			this.point = val;
		}


	}
}
