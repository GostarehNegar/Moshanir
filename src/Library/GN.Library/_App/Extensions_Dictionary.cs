using System;
using System.Collections.Generic;
using System.Text;

namespace GN
{
    public static partial class Extensions
    {
        public static T AnonymousCast<T>(object obj, T type)
        {
            return (T)obj;
        }
        private static string GetKey(this Type type, string key)
        {
            return string.Format("{0}|{1}", type?.FullName, key);
        }
        /// <summary>
        /// Gets an object value from an object collection.
        /// Note thate object collections store keys as "type|key" pairs. 
        /// Therefore if key is missing the object is stored under its type name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetObjectValue<T>(this IDictionary<string, object> dic, string key = null)
        {
            return TryGetObjectValue<T>(dic, out var _ret, key)
                ? _ret
                : default(T);
        }
        public static T GetOrAddObjectValue<T>(this IDictionary<string, object> dic, Func<T> constructor = null, string key = null)
        {
            T result = default(T);
            //key = string.Format("{0}|{1}", typeof(T).FullName, key);
            key = typeof(T).GetKey(key);
            if (dic.TryGetValue(key, out var ret))
            {
                if (ret == null)
                    return default(T);
                if (typeof(T).IsAssignableFrom(ret.GetType()))
                    return (T)ret;
                if (ret != null)
                {
                    //try
                    //{
                    //	var str = ret.ToString();
                    //	if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?) && !str.StartsWith("\""))
                    //		str = "\"" + ret.ToString() + "\"";
                    //	ret = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
                    //	if (typeof(T).IsAssignableFrom(ret.GetType()))
                    //	{
                    //		dic[key] = ret;
                    //		return (T)ret;
                    //	}
                    //}
                    //catch { }
                }
                result = constructor == null ? result : constructor();
                dic[key] = result;
            }
            else
            {
                result = constructor == null ? result : constructor();
                dic.Add(key, result);
            }
            return result;
        }


        /// <summary>
        /// Gets or adds an object value from an object collection.
        /// Note thate object collections store keys as "type|key" pairs. 
        /// Therefore if key is missing the object is stored under its type name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetOrAddObjectValueWithDeserialization<T>(this IDictionary<string, object> dic, Func<T> constructor = null, string key = null)
        {
            T result = default(T);
            //key = string.Format("{0}|{1}", typeof(T).FullName, key);
            key = typeof(T).GetKey(key);
            if (dic.TryGetValue(key, out var ret))
            {
                if (ret == null)
                    return default(T);
                if (typeof(T).IsAssignableFrom(ret.GetType()))
                    return (T)ret;
                if (ret != null)
                {
                    try
                    {
                        var str = ret.ToString();
                        if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?) && !str.StartsWith("\""))
                            str = "\"" + ret.ToString() + "\"";
                        ret = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
                        if (typeof(T).IsAssignableFrom(ret.GetType()))
                        {
                            dic[key] = ret;
                            return (T)ret;
                        }
                    }
                    catch { }
                }
                result = constructor == null ? result : constructor();
                dic[key] = result;
            }
            else
            {
                result = constructor == null ? result : constructor();
                dic.Add(key, result);
            }
            return result;
        }

        /// <summary>
        /// Adds or updates an object value from an object collection.
        /// Note thate object collections store keys as "type|key" pairs. 
        /// Therefore if key is missing the object is stored under its type name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T AddOrUpdateObjectValue<T>(this IDictionary<string, object> dic, Func<T> constructor, string key = null)
        {
            T result = default(T);
            //key = string.Format("{0}|{1}", typeof(T).FullName, key);
            key = typeof(T).GetKey(key);
            if (dic.TryGetValue(key, out var ret))
            {
                result = constructor == null ? result : constructor();
                dic[key] = result;
            }
            else
            {
                result = constructor == null ? result : constructor();
                dic.Add(key, result);
            }
            return result;
        }

        /// <summary>
        /// Removes an object value from an object collection.
        /// Note thate object collections store keys as "type|key" pairs. 
        /// Therefore if key is missing the object is stored under its type name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static void RemoveObjectValue<T>(this IDictionary<string, object> dic, string key = null)
        {
            //key = string.Format("{0}|{1}", typeof(T).FullName, key);
            key = typeof(T).GetKey(key);
            if (dic.ContainsKey(key))
                dic.Remove(key);
        }

        /// <summary>
        /// Try get value from an object collection.
        /// Note thate object collections store keys as "type|key" pairs. 
        /// Therefore if key is missing the object is stored under its type name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool TryGetObjectValue<T>(this IDictionary<string, object> dic, out T value, string key = null)
        {
            key = typeof(T).GetKey(key);
            value = default(T);
            if (dic.TryGetValue(key, out var ret))
            {
                if (ret == null)
                    return true;
                if (ret != null && typeof(T).IsAssignableFrom(ret.GetType()))
                {
                    value = (T)ret;
                    return true;
                }
            }
            return false;

        }




        public static bool TrySetValue<T>(this IDictionary<string, string> dic, string key, T value)
        {
            string string_value = null;
            if (value != null)
            {
                if (typeof(T) == typeof(string))
                {
                    string_value = value.ToString();
                }
                else
                {
                    string_value = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                }
            }

            if (dic.ContainsKey(key))
                dic[key] = string_value;
            else
                dic.Add(key, string_value);

            return true;

        }
        public static bool TryGetValue<T>(this IDictionary<string, string> dic, string key, out T result)
        {
            result = default(T);
            if (dic.TryGetValue(key, out var _result))
            {
                if (_result == null)
                {
                    if (IsNullable(typeof(T)))
                    {
                        result = (T)(object)null;
                    }
                    return true;
                }
                if (typeof(T).IsAssignableFrom(_result.GetType()))
                {
                    result = (T)(object)_result;
                    return true;
                }
                if ((typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?)) && !_result.StartsWith("\""))
                    _result = "\"" + _result.ToString() + "\"";
                try
                {
                    result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(_result);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public static T GetValue<T>(this IDictionary<string, string> dic, string key)
        {
            return dic.TryGetValue<T>(key, out var result)
                ? result
                : default;
        }


        public static bool TryGetValue<T>(this IDictionary<string, object> dic, string key, out T value, Func<T> ctor = null)
        {
            key = key ?? typeof(T).FullName;
            value = default(T);
            if (dic.TryGetValue(key, out var ret))
            {
                if (ret == null)
                    return true;
                if (ret != null && typeof(T).IsAssignableFrom(ret.GetType()))
                {
                    value = (T)ret;
                    return true;
                }
            }
            else if (ctor != null)
            {
                lock (dic)
                {
                    if (dic.TryGetValue(key, out ret))
                    {
                        if (ret == null)
                            return true;
                        if (ret != null && typeof(T).IsAssignableFrom(ret.GetType()))
                        {
                            value = (T)ret;
                            return true;
                        }
                    }
                    value = ctor();
                    dic[key] = value;
                }
                return true;
            }
            return false;
        }
        public static T GetValue<T>(this IDictionary<string, object> dic, string key, Func<T> ctor = null)
        {
            return dic.TryGetValue<T>(key, out var _res, ctor) ? _res : default(T);
        }
        public static T SetValue<T>(this IDictionary<string, object> dic, string key, Func<T> ctor = null)
        {
            dic.RemoveValue<T>(key);
            return dic.TryGetValue<T>(key, out var _res, ctor) ? _res : default(T);
        }
        public static T RemoveValue<T>(this IDictionary<string, object> dic, string key)
        {
            dic.TryGetValue<T>(key, out var _res);
            dic.Remove(key ?? typeof(T).FullName);
            return _res;
        }



    }
}
