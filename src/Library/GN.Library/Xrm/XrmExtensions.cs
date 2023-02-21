using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using GN.Library.Configurations;

namespace GN.Library.Xrm
{
	public static partial class XrmExtensions
	{
		//public static string GetRawXrmConnectionString(this IAppConfiguration This)
		//{
		//	var result = This.Configuration.GetConnectionString("Xrm");
		//	if (!string.IsNullOrWhiteSpace(result))
		//		XrmSettings.Current.ConnectionString = result;
		//	result = XrmSettings.Current.ConnectionString;
		//	return result;
		//}
		//public static XrmConnectionString GetXrmConnectionString(this IAppConfiguration This, Action<XrmConnectionString> configure = null)
		//{
		//	if (configure == null)
		//	{
		//		return
		//			This.GetOrAddValue<XrmConnectionString>(null, x =>
		//			{
		//				return x.Settings?.Parent?.GetXrmConnectionString()?.Clone() ??
		//					new XrmConnectionString(This);
		//			});
		//	}
		//	return This.Update<XrmConnectionString>(null, x =>
		//	{
		//		var val = x.GetCurrentValue<XrmConnectionString>();
		//		var result = x.Settings.Parent?.GetXrmConnectionString()?.Clone() ?? new XrmConnectionString(This);
		//		configure?.Invoke(result);
		//		return result;
		//	});
		//}

		public static void AddDataServices(this IServiceCollection services)
		{
			//services.AddTransient<IXrmOrganizationService, XrmOrganizationService>();
			//services.AddTransient<IXrmRepository, XrmRepository>();
			//services.AddTransient(typeof(IXrmRepository<>), typeof(XrmRepository<>));
			//services.AddTransient<XrmConnectionString>(x => { return x.GetService<IAppConfiguration>().GetXrmConnectionString(); });
		}
		//public static string GetRawXrmConnectionString_Deprecated(this IAppConfiguration This)
		//{
  //          var result =This.Configuration.GetConnectionString("Xrm");
  //          return result;
		//}
		//public static XrmConnectionString GetXrmConnectionString_Deprecated(this IAppConfiguration This, Action<XrmConnectionString> configure = null)
		//{
		//	if (configure == null)
		//	{
		//		return 
		//			This.GetOrAddValue<XrmConnectionString>(null, x => 
		//			{
		//				return x.Settings?.Parent?.GetXrmConnectionString_Deprecated()?.Clone()??
		//					new XrmConnectionString(This);
		//			});
		//	}
		//	return This.Update<XrmConnectionString>(null, x =>
		//	{
		//		var val = x.GetCurrentValue<XrmConnectionString>();
		//		var result = x.Settings.Parent?.GetXrmConnectionString_Deprecated()?.Clone() ?? new XrmConnectionString(This);
		//		configure?.Invoke(result);
		//		return result;
		//	});
		//}


	}
}
