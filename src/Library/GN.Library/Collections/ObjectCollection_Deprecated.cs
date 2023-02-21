using GN.Library.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Collections
{

	public interface IObjectCollection_Deprecated
	{
		bool TryGetOrAddFactory<T>(string key, Func<T> factory, bool serializable, out T value);
		bool TryGetOrAdd<T>(string key, T defaultValue, bool serializable, out T value);
		bool TryUpdate<T>(string key, T value, bool serializable = false);
		bool TryDelete<T>(string key);
		T GetOrAddFactory<T>(string key, Func<T> factory = null, bool serializable = false);
		T GetOrAdd<T>(string key, T value = default(T), bool serializable = false);
		T Get<T>(string key);
		T Delete<T>(string key = null);
		void Update<T>(string key, T value, bool serializable = false);
		int Count<T>(string key = null);
		IObjectCollection_Deprecated Clone();
	}

	[Newtonsoft.Json.JsonConverter(typeof(TypedJsonConverter<ObjectCollection_Deprecated>))]
	public class ObjectCollection_Deprecated : IObjectCollection_Deprecated
	{
		public class ObjectEntry
		{
			[JsonProperty("K")]
			public string Key { get; set; }
			[JsonProperty("V")]
			public object Value { get; set; }
			//[JsonIgnore]
			[JsonProperty("S")]
			public bool Serializable { get; set; }

			public ObjectEntry(string key, object value, bool isPersistable)
			{
				this.Key = key;
				this.Value = value;
				this.Serializable = isPersistable;

			}
			public ObjectEntry Reset(string key, object value, bool serializable)
			{
				this.Key = key;
				this.Value = value;
				this.Serializable = serializable;
				return this;
			}

			public bool Matches(string key, Type type)
			{
				bool exact = key == null;
				if (type == null)
					return key != null && string.Compare(this.Key, key, true) == 0;
				var keyMatch = key == null || string.Compare(this.Key, key, true) == 0;
				if (exact)
					return keyMatch && this.Value?.GetType() == type;
				if (type.IsInterface)
				{
					return keyMatch && this.Value != null && _IsAssignableFromEx(type, this.Value?.GetType());
				}
				return keyMatch && this.Value != null && _IsConvertibleTo(this.Value, type);
			}

			public bool TryCast(Type type, out object value)
			{
				return TryConvertWithJsonSupport(type, this.Value, out value);
			}
			public T Cast<T>()
			{
				return TryCast(typeof(T), out var value)
					? (T)value
					: default(T);

			}

			private static bool _IsAssignableFromEx(Type interfaceType, Type concreteType)
			{
				var result = concreteType == null || interfaceType == null
					? false
					: interfaceType == concreteType
					? true
					: !interfaceType.IsGenericType
						? interfaceType.IsAssignableFrom(concreteType)
						: interfaceType.IsGenericTypeDefinition
							? concreteType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType).FirstOrDefault() != null
							: concreteType.GetInterfaces().Where(x => x == interfaceType).FirstOrDefault() != null;
				return result;
			}
			private static bool _IsGuid(string str)
			{
				return Guid.TryParse(str, out var result);
			}
			private static bool _IsConvertibleTo(object value, Type desiredType)
			{
				if (desiredType == null)
					return false;
				if (value == null)
					return desiredType.IsClass || Nullable.GetUnderlyingType(desiredType) != null;
				var sourceType = value.GetType();
				if (desiredType.IsAssignableFrom(sourceType))
					return true;
				if (sourceType == typeof(string) && (desiredType == typeof(Guid) || desiredType == typeof(Guid?)))
					return _IsGuid((string)value);
				if (sourceType == typeof(long) && (desiredType == typeof(int) || desiredType == typeof(int?)))
					return true;
				if (sourceType == typeof(Newtonsoft.Json.Linq.JObject) && typeof(object).IsAssignableFrom(desiredType))
					return true;
				if (sourceType == typeof(Newtonsoft.Json.Linq.JArray) && typeof(object).IsAssignableFrom(desiredType))
					return true;
				return false;
			}
			public static bool TryConvertWithJsonSupport(Type type, object src, out object result)
			{
				result = null;
				if (type == null)
					throw new InvalidOperationException(
						"Type cannot be NULL.");
				if (src == null)
				{
					return type.IsClass || Nullable.GetUnderlyingType(type) != null;
				}

				var srcType = src.GetType();
				if (type.IsAssignableFrom(srcType))
				{
					result = src;
					return true;
				}
				if ((type == typeof(Guid) || type == typeof(Guid?)) && srcType == typeof(string))
				{
					Guid parse;
					if (Guid.TryParse(src.ToString(), out parse))
					{
						result = parse;
						return true;
					}
					return false;
				}
				if (srcType == typeof(long))
				{
					if (type == typeof(int) || type == typeof(int?))
					{
						result = (int)Convert.ToInt32((long)src);
						return true;
					}
				}

				if (srcType == typeof(Newtonsoft.Json.Linq.JObject) || srcType == typeof(Newtonsoft.Json.Linq.JArray))
				{
					try
					{
						var parse = Newtonsoft.Json.JsonConvert.DeserializeObject(src.ToString(), type);
						result = parse;
						return true;
					}
					catch { return false; }
				}
				return false;
			}
		}

		[JsonProperty("Items")]
		private List<ObjectEntry> items = new List<ObjectEntry>();
		[JsonIgnore]
		private List<ObjectEntry> _serializtionItemsBackup;
		[JsonIgnore]
		private object _lock = new object();
		public ObjectCollection_Deprecated()
		{

		}
		private bool Try(Type type, string key, Func<ObjectEntry, ObjectEntry> factory, bool add, bool delete, bool overwrite, out ObjectEntry entry)
		{
			entry = null;
			var keepUinque = false;
			bool result = true;
			var matched = this.items.Where(x => x.Matches(key, type)).ToList();
			if ((delete || overwrite || keepUinque) && matched.Count > 0)
			{
				lock (this._lock)
				{
					matched.ForEach(x => this.items.Remove(x));
				}
				result = delete;
				entry = matched.LastOrDefault();
				matched = new List<ObjectEntry>();
			}
			if ((overwrite || (add && matched.Count == 0)) && factory != null)
			{
				entry = new ObjectEntry(key, null, false);
				entry = factory(entry);
				if (entry.Value != null)
				{
					lock (this._lock)
					{
						this.items.Add(entry);
					}
					result = true;
				}
			}
			else if (matched.Count > 0)
			{
				entry = matched.LastOrDefault();
			}
			return result;
		}
		private bool TryGetOrAdd<T>(string key, Func<ObjectEntry, ObjectEntry> factory, out T value)
		{
			var result = this.Try(typeof(T), key, factory, true, false, false, out var entry) && entry != null;
			value = result ? entry.Cast<T>() : default(T);
			return result;


		}
		public bool TryGetOrAddFactory<T>(string key, Func<T> factory, bool serializable, out T value)
		{
			Func<ObjectEntry, ObjectEntry> _fact = factory == null
				? (Func<ObjectEntry, ObjectEntry>)null
				: x =>
					 {
						 return x.Reset(key, factory(), serializable);
					 };
			var result = this.Try(typeof(T), key, _fact, true, false, false, out var entry) && entry != null;
			value = result ? entry.Cast<T>() : default(T);
			return result;
		}
		public bool TryGetOrAdd<T>(string key, T defaultValue, bool serializable, out T value)
		{
			return this.TryGetOrAddFactory<T>(key, () => { return defaultValue; }, serializable, out value);
		}



		public bool TryUpdate<T>(string key, T value, bool serializable = false)
		{
			return Try(typeof(T), key,
				x => x.Reset(key, value, serializable)
				, false, value == null, value != null, out var entry);
		}
		public bool TryDelete<T>(string key)
		{
			return Try(typeof(T), key, null, false, true, false, out var entry);
		}

		public T GetOrAddFactory<T>(string key, Func<T> factory, bool serializable = false)
		{
			return this.TryGetOrAddFactory<T>(key, factory, serializable, out var value) ? value : default(T);
		}
		public T GetOrAdd<T>(string key, T value = default(T), bool serializable = false)
		{
			return this.TryGetOrAdd<T>(key, value, serializable, out var v) ? v : default(T);
		}
		public T Delete<T>(string key)
		{
			var result = this.Try(typeof(T), key, null, false, true, false, out var entry) && entry != null;
			return result ? entry.Cast<T>() : default(T);
		}

		public void Update<T>(string key, T value, bool serializable = false)
		{

			this.TryUpdate<T>(key, value, serializable);
		}
		public int Count<T>(string key = null)
		{
			return this.items.Count(x => x.Matches(key, typeof(T)));
		}

		public T Get<T>(string key)
		{
			return this.TryGetOrAddFactory<T>(key, null, false, out var result)
				? result
				: default(T);
		}


		[OnSerialized]
		internal void OnSerialized(StreamingContext context)
		{
			if (_serializtionItemsBackup != null)
			{
				this.items = this._serializtionItemsBackup;
			}
			Monitor.Exit(this._lock);
		}
		[OnSerializing]
		internal void OnSerializing(StreamingContext context)
		{
			Monitor.Enter(this._lock);
			this._serializtionItemsBackup = this.items;
			this.items = this.items.Where(x => x.Serializable).ToList();

		}
		[OnDeserializing]
		internal void OnDeserializing(StreamingContext context)
		{
			//this._serializtionItemsBackup = this.items;
			//this.items = this.items.Where(x => x.Persistable.HasValue && x.Persistable.Value).ToList();

		}
		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context)
		{
			//this._serializtionItemsBackup = this.items;
			//this.items = this.items.Where(x => x.Persistable.HasValue && x.Persistable.Value).ToList();

		}

		public IObjectCollection_Deprecated Clone()
		{
			var result = new ObjectCollection_Deprecated
			{
				items = this.items.ToArray().ToList()
			};
			return result;
		}

	}
}
