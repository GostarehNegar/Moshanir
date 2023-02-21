using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Linq;

namespace GN.Library.Messaging.Internals
{
    public class MessageTopicHelper
    {
        private static ConcurrentDictionary<string, Type> nameTypeMap = new ConcurrentDictionary<string, Type>();
        private static ConcurrentDictionary<Type, string> typeNameMap = new ConcurrentDictionary<Type, string>();
        public static void Register(Type type, string name)
        {
            nameTypeMap.GetOrAdd(name, type);
            typeNameMap.GetOrAdd(type, name);
        }
        public static string GetTopicByType(Type type)
        {
            return type.Name;
            return typeNameMap.TryGetValue(type, out var _val)
                ? _val
                : GetNameFromType(type);
        }
        internal static string GetNameFromType(Type type)
        {
            var result = type?.Name;
            if (!string.IsNullOrWhiteSpace(result) && result.StartsWith("Contracts."))
            {
                result.Remove(0, "Contracts.".Length);
            }
            return result;
        }
        private static int count_matches(string source, string target)
        {
            var result = 0;
            source.Split('.')
                .ToList()
                .ForEach(x =>
                {
                    result = target.Contains(x) ? result + 1 : result;
                });
            return result;
        }
        private static string NET_CORE_LIB = ", System.Private.CoreLib";
        private static string MSCORE_LIB = ", mscorlib";
        private static string FixForSystemCore(string name)
        {
            var idx = name.IndexOf(NET_CORE_LIB);
            if (idx > 0)
            {
                return name.Substring(0, idx);
            }
            idx = name.IndexOf(MSCORE_LIB);
            if (idx > 0)
            {
                return name.Substring(0, idx);
            }
            return name;
        }

        public static bool TryGetTypeByName(string name, out Type result)
        {
            result = nameTypeMap.TryGetValue(name, out var _val)
                ? _val
                : null;
            if (result == null)
            {
                result = Type.GetType(FixForSystemCore(name));
                if (result != null)
                {
                    Register(result, name);
                }
            }
            if (result == null && !name.Contains(','))
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .OrderByDescending(x => count_matches(name, x.GetName().Name))
                .Where(x => count_matches(name, x.GetName().Name) > 0)
                .ToList();
                foreach (var asm in assemblies)
                {
                    result = Type.GetType($"{name},{asm.FullName}");
                    if (result != null)
                        break;
                }
                if (result != null)
                {
                    Register(result, name);
                }
            }
            if (result == null)
            {
                Register(typeof(KeyNotFoundException), name);
            }
            result = result == typeof(KeyNotFoundException) ? null : result;
            return result != null;
        }
        public static Type GetTypeByName(string name)
        {
            if (name == null)
            {
                return null;
            }
            var result = nameTypeMap.TryGetValue(name, out var _val)
                ? _val
                : null;
            if (result == null)
            {
                result = Type.GetType(FixForSystemCore(name));
                if (result != null)
                {
                    Register(result, name);
                }
            }
            if (result == null && !name.Contains(','))
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .OrderByDescending(x => count_matches(name, x.GetName().Name))
                .Where(x => count_matches(name, x.GetName().Name) > 0)
                .ToList();
                foreach (var asm in assemblies)
                {
                    result = Type.GetType($"{name},{asm.FullName}");
                    if (result != null)
                        break;
                }
                if (result != null)
                {
                    Register(result, name);
                }
            }
            if (result == null)
            {
                Register(typeof(KeyNotFoundException), name);
            }
            return result == typeof(KeyNotFoundException) ? null : result;
        }
    }
}
