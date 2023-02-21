using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Serialization
{
    public interface TypedDictionary :
        IDictionary<string, object>
    {
        T GetValue<T>(string key = null);
        void AddOrUpdate<T>(T value, string key, Func<T> valueFunc);
        bool TryGetValue<T>(string key, out object result);
        void Remove<T>(string key = null);
    }
}
