using GN.Library.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Collections
{

	public interface ICloneableDictionary
	{
		bool TryGetValue<T>(string key, out T value);
		void RemoveValue<T>(string key);
		void AddOrUpdate<T>(T value, string key = null);
		T GetOrAddValue<T>(Func<T> factory, string key = null);
		T GetValue<T>(string key = null);
		ICloneableDictionary Clone();
	}
	public class CloneableDictionary : ICloneableDictionary
	{
		private ConcurrentDictionary<string, string> headers = new ConcurrentDictionary<string, string>();
		private ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();
		public CloneableDictionary(IEnumerable<KeyValuePair<string, string>> collecion = null)
		{
			this.headers = collecion == null
				? new ConcurrentDictionary<string, string>()
				: new ConcurrentDictionary<string, string>(collecion);

		}
		private static string GetKey(Type type, string key)
		{
			return string.Format("{0}|{1}", type?.FullName, key);
		}

		public bool TryGetValue<T>(string key, out T value)
		{
			var _key = GetKey(typeof(T), key);
			if (this.cache.TryGetValue(_key, out var tmp))
			{
				if (tmp != null && typeof(T).IsAssignableFrom(tmp.GetType()))
				{
					value = (T)tmp;
					return true;
				}
				else if (tmp == null && typeof(T).IsNullable())
				{
					value = (T)tmp;
					return true;
				}

			}
			if (this.headers.TryGetValue(_key, out var _tmp) && _tmp != null)
			{
				try
				{
					var _value = Deserialize<T>(_tmp);
					this.cache.AddOrUpdate(_key, _value, (a, b) => _value);
					value = _value;
					return true;
				}
				catch { }
			}
			value = default(T);
			return false;
		}
		public void AddOrUpdate<T>(T value, string key = null)
		{
			var _key = GetKey(typeof(T), key);
			if (value != null)
			{
				this.cache.AddOrUpdate(_key, value, (a, b) => value);
				var str_value = Serialize(value);
				this.headers.AddOrUpdate(_key, str_value, (a, b) => str_value);
			}
		}
		public void RemoveValue<T>(string key)
		{
			var _key = GetKey(typeof(T), key);
			this.cache.TryRemove(_key, out var _);
			this.headers.TryRemove(_key, out var _);
		}
		public T GetOrAddValue<T>(Func<T> factory, string key = null)
		{
			T result = default(T);
			if (TryGetValue<T>(key, out result))
				return result;
			if (factory != null)
				result = factory();
			if (result != null)
			{
				AddOrUpdate<T>(result, key);
			}
			return result;
		}

		public ICloneableDictionary Clone()
		{
			foreach (var item in this.headers)
			{
				if (this.cache.TryGetValue(item.Key, out var tmp))
				{
					this.headers[item.Key] = Serialize(tmp);
				}
			}
			return new CloneableDictionary(this.headers);
		}
		private static string Serialize(object value)
		{
			if (value == null)
				return null;
			return value.GetType().IsAbstract
				? $"{value.GetType().AssemblyQualifiedName}%{JsonConvert.SerializeObject(value)}"
				: $"{value.GetType().AssemblyQualifiedName}%{JsonConvert.SerializeObject(value)}";
		}
		private static T Deserialize<T>(string value)
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

		public T GetValue<T>(string key = null)
		{
			return TryGetValue<T>(key, out var tmp)
				? tmp
				: default;
		}
	}
}
