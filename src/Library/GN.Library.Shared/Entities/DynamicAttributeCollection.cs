using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GN.Library.Shared.Entities
{
    public class DynamicAttributeCollection : ConcurrentDictionary<string, string>
    {
        private ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();
        public DynamicAttributeCollection() : base() { }

        public DynamicAttributeCollection(IDictionary<string, string> items) : base(items?? new Dictionary<string,string>()) { }
        private static string Serialize(object value)
        {
            if (value == null)
                return null;
            if (value.GetType() == typeof(string))
            {
                return value.ToString();
            }
            return JsonConvert.SerializeObject(value);
        }
        private static T Deserialize<T>(string value)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)(value);
            }
            return string.IsNullOrEmpty(value)
                ? default(T)
                : JsonConvert.DeserializeObject<T>(value);

        }
        private static string GetKey(Type type, string key)
        {
            return key;
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
                else if (tmp == null && IsNullable(typeof(T)))
                {
                    value = (T)tmp;
                    return true;
                }

            }
            if (this.TryGetValue(_key, out var _tmp) && _tmp != null)
            {
                if (_tmp != null && typeof(T).IsAssignableFrom(_tmp.GetType()))
                {
                    value = (T) (object) _tmp;
                    return true;
                }
                else if (_tmp == null && IsNullable(typeof(T)))
                {
                    value = default(T);
                    return true;
                }
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
        private static bool IsNullable(Type type)
        {
            return type == null || !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }
        public void AddOrUpdate(string key, object value)
        {
            var _key = GetKey(typeof(string), key);
            if (value != null)
            {
                this.cache.AddOrUpdate(_key, value, (a, b) => value);
                var str_value = Serialize(value);
                this.AddOrUpdate(_key, str_value, (a, b) => str_value);
            }
        }
        public void AddOrUpdate<T>(string key , T value)
        {
            var _key = GetKey(typeof(T), key);
            if (value != null)
            {
                this.cache.AddOrUpdate(_key, value, (a, b) => value);
                var str_value = Serialize(value);
                this.AddOrUpdate(_key, str_value, (a, b) => str_value);
            }
        }
        public void RemoveValue<T>(string key)
        {
            var _key = GetKey(typeof(T), key);
            this.cache.TryRemove(_key, out var _);
            this.TryRemove(_key, out var _);
        }

        public void Add( string key, string value)
        {
            this.AddOrUpdate(key, value);
        }
    }

    public class DynamicPropertyCollection : ConcurrentDictionary<string, string>
    {
        private ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();
        public DynamicPropertyCollection() : base() { }

        public DynamicPropertyCollection(IDictionary<string, string> items) : base(items ?? new Dictionary<string, string>()) { }
        private static string Serialize(object value)
        {
            if (value == null)
                return null;
            if (value.GetType() == typeof(string))
            {
                return value.ToString();
            }
            return JsonConvert.SerializeObject(value);
        }
        private static T Deserialize<T>(string value)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)(value);
            }
            return string.IsNullOrEmpty(value)
                ? default(T)
                : JsonConvert.DeserializeObject<T>(value);

        }
        private static string GetKey(Type type, string key)
        {
            return key;// string.Format("{0}|{1}", type?.FullName, key);
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
                else if (tmp == null && IsNullable(typeof(T)))
                {
                    value = (T)tmp;
                    return true;
                }

            }
            if (this.TryGetValue(_key, out var _tmp) && _tmp != null)
            {
                if (_tmp != null && typeof(T).IsAssignableFrom(_tmp.GetType()))
                {
                    value = (T)(object)_tmp;
                    return true;
                }
                else if (_tmp == null && IsNullable(typeof(T)))
                {
                    value = default(T);
                    return true;
                }
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
        private static bool IsNullable(Type type)
        {
            return type == null || !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }
        public void AddOrUpdate(string key, object value)
        {
            var _key = GetKey(typeof(string), key);
            if (value != null)
            {
                this.cache.AddOrUpdate(_key, value, (a, b) => value);
                var str_value = Serialize(value);
                this.AddOrUpdate(_key, str_value, (a, b) => str_value);
            }
        }
        public void AddOrUpdate<T>(string key, T value)
        {
            var _key = GetKey(typeof(T), key);
            if (value != null)
            {
                this.cache.AddOrUpdate(_key, value, (a, b) => value);
                var str_value = Serialize(value);
                this.AddOrUpdate(_key, str_value, (a, b) => str_value);
            }
        }
        public void RemoveValue<T>(string key)
        {
            var _key = GetKey(typeof(T), key);
            this.cache.TryRemove(_key, out var _);
            this.TryRemove(_key, out var _);
        }

        public void Add(string key, string value)
        {
            this.AddOrUpdate(key, value);
        }
    }

    public class DynamicEntityCollection : ConcurrentDictionary<string, DynamicEntity>
    {
        public DynamicEntityCollection() : base() { }
        public DynamicEntityCollection(IDictionary<string, DynamicEntity> items) : base(items ?? new Dictionary<string, DynamicEntity>()) { }
    }
}
