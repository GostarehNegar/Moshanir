using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Helpers
{
	
	public static class Converter
	{
		public static T Convert<T>(object source, bool useSerialization = true)
		{
			return TryConvert<T>(source, out var _d, useSerialization) ? _d : default(T);
		}
		public static bool TryConvert<T>(object source, out T dest, bool useSerialization = true)
		{
			dest = default(T);
			var result = TryConvert(source, typeof(T), out var _dest, useSerialization);
			if (result)
				dest = (T)_dest;
			return result;
		}
		public static bool IsNullableEx(this Type type)
		{
			return type == null || !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
		}
		public static bool TryConvert(object source, Type type, out object result, bool useSerialization = true)
		{
			result = null;
			if (source == null && type.IsNullableEx())
				return true;
			if (source == null)
				return false;
			if (type == null)
				return false;
			var sourceType = source.GetType();
			if (type.IsAssignableFrom(source.GetType()))
			{
				result = source;
				return true;
			}
			if (type == typeof(string))
			{
				if (!sourceType.IsValueType && sourceType != typeof(string) && useSerialization)
				{
					try
					{
						result = Newtonsoft.Json.JsonConvert.SerializeObject(source);
						return true;
					}
					catch { }
				}
				result = source.ToString();
				return true;
			}
			if ((type == typeof(int) || type == typeof(int?)) && int.TryParse(source.ToString(), out var tmp))
			{
				result = tmp;
				return true;
			}
			if ((type == typeof(Guid?) || type == typeof(Guid)) && Guid.TryParse(source.ToString(), out var _tmp))
			{
				result = _tmp;
				return true;
			}
			if ((type == typeof(DateTime?) || type == typeof(DateTime)) && DateTime.TryParse(source.ToString(), out var __tmp))
			{
				result = __tmp;
				return true;
			}
			/// Try using Newtonsoft
			/// 
			if (useSerialization)
			{
				var _result = source.ToString();
				if ((type == typeof(Guid) || type == typeof(Guid?)) && !_result.StartsWith("\""))
					_result = "\"" + _result.ToString() + "\"";
				try
				{
					result = Newtonsoft.Json.JsonConvert.DeserializeObject(_result, type);
					return true;
				}
				catch { }
			}
			return false;
		}
	}
}
