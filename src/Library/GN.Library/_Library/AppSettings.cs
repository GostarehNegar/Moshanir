using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace GN.Library
{
    public interface IPersistableConfiguration
    {
    }
    public class AppSettings : IPersistableConfiguration
    {
        protected static string GetPath(Type type)
        {
            var path = Path.GetFullPath(
                            Path.Combine(
                            Path.GetDirectoryName(type.Assembly.Location),
                            "Settings/" + $"{type.FullName}.json"));
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;

        }
        protected static bool _Save(Type type, object value)
        {
            var path = GetPath(type);
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            var txt = Newtonsoft.Json.JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(path, txt, Encoding.UTF8);
            return File.Exists(path);
        }
        public static bool Save(Type type, object value)
        {
            return _Save(type, value);
        }
    }
    public class AppSettings<T> : AppSettings
    {

        protected static T _default;
        protected static T current;
        public static T Default
        {
            get
            {
                if (_default == null)
                {
                    _default = Activator.CreateInstance<T>();
                }
                return _default;
            }

        }
        public static T Current
        {
            get
            {
                if (current == null)
                {
                    current = Load(Default);
                    if (AppHost_Deprectated.Initialized)
                        current = AppHost_Deprectated.Configuration.GetOrAddValue<T>(null, x => current);

                }
                return current;
            }
        }
        public static T Load(T defaultValue)
        {
            T result = defaultValue;
            try
            {
                if (result == null)
                {
                    try { result = Activator.CreateInstance<T>(); } catch { }
                }
                var path = GetPath(typeof(T));
                if (File.Exists(path))
                {
                    var txt = File.ReadAllText(path, Encoding.UTF8);
                    result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(txt);
                }
                else if (result != null)
                {
                    _Save(typeof(T), result);
                }
            }
            catch { }
            return result;


        }
        public bool Save()
        {
            return _Save(this.GetType(), this);
        }
    }

}
