using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GN.Library.Shared.Entities
{
    public class SimpleDictionary : ConcurrentDictionary<string, string>
    {
        
        public SimpleDictionary()
        {

        }
        public SimpleDictionary(IDictionary<string, string> headers) : base(headers)
        {

        }
        public bool TrySetValue(string key, object value)
        {
            string strValue = null;
            switch (value)
            {
                case int iVal:
                    strValue = iVal.ToString();
                    break;
                case decimal dVal:
                    strValue = dVal.ToString();
                    break;
                case double dVal:
                    strValue = dVal.ToString();
                    break;
                case string sVal:
                    strValue = sVal;
                    break;
                case Guid guidValue:
                    strValue = guidValue.ToString();
                    break;
                case long lValue:
                    strValue = lValue.ToString();
                    break;
                case DateTime dt:
                    strValue = dt.ToString();
                    break;

                case null:
                    strValue = null;
                    break;
                default:
                    break;
            }
            this.AddOrUpdate(key, strValue, (a, b) => strValue);
            return true;
        }
        public bool TryGetValue(string key, Type type, out object result)
        {
            if (this.TryGetValue(key, out var _res))
            {
                if (type == typeof(string))
                {
                    result = _res;
                    return true;
                }
                if (_res == null && (!type.IsValueType || Nullable.GetUnderlyingType(type) != null))
                {
                    result = null;
                    return true;
                }
                else if ((type == typeof(int) || type == typeof(int?)) && int.TryParse(_res, out var iVal))
                {
                    result = iVal;
                    return true;
                }
                else if ((type == typeof(long) || type == typeof(long?)) && long.TryParse(_res, out var lVal))
                {
                    result = lVal;
                    return true;
                }
                else if ((type == typeof(Guid) || type == typeof(Guid?)) && Guid.TryParse(_res, out var gVal))
                {
                    result = gVal;
                    return true;
                }
                else if ((type == typeof(decimal) || type == typeof(decimal?)) && decimal.TryParse(_res, out var dcVal))
                {
                    result = dcVal;
                    return true;
                }
                else if ((type == typeof(double) || type == typeof(double?)) && decimal.TryParse(_res, out var doVal))
                {
                    result = doVal;
                    return true;
                }
                else if ((type == typeof(DateTime) || type == typeof(DateTime?)) && DateTime.TryParse(_res, out var dVal))
                {
                    result = dVal;
                    return true;
                }
            }
            result = null;
            return false;
        }

        public bool TryGetValue<T>(string key, out T result)
        {
            var type = typeof(T);
            if (this.TryGetValue(key, typeof(T), out var _res))
            {
                if (_res == null && (!type.IsValueType || Nullable.GetUnderlyingType(type) != null))
                {
                    result = default(T);
                    return true;
                }
                if (_res != null && type.IsAssignableFrom(_res.GetType()))
                {
                    result = (T)_res;
                    return true;
                }
            }
            result = default;
            return false;
        }

        public T GetValue<T>(string key)
        {
            return TryGetValue<T>(key, out var result) ? result : default(T);
        }

        public bool TrySetValue<T>(string key, T value)
        {
            return this.TrySetValue(key, (object)value);
        }
    }
}
