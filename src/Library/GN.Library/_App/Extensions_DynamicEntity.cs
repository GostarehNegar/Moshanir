using GN.Library.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library
{
	public static partial class Extensions
	{
		public static T GetAttributeValue<T>(this IDynamicEntity entity, string name)
		{
			var result = default(T);
			if (entity.Attributes.TryGetValue(name,out var _value))
			{
				if (_value != null && typeof(T).IsAssignableFrom(_value.GetType()))
				{
					result = (T)_value;
				}
			}
			return result;
		}
		public static IDynamicEntity SetAttributeValue<T>(this IDynamicEntity entity, string name, T value)
		{
			var result = default(T);
			if (entity.Attributes.ContainsKey(name))
			{
				entity.Attributes[name] = value;
			}
			else
			{
				entity.Attributes.Add(name, value);
			}
			
			return entity;
		}
	}
}
