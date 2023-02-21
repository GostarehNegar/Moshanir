#define NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Collections.Concurrent;
using System.Reflection;

namespace GN.Library.Helpers
{
    public class ProxyBuilder
    {
        const MethodAttributes PropertyAccessMethodAttributes = MethodAttributes.Public
        | MethodAttributes.SpecialName
        | MethodAttributes.HideBySig
        | MethodAttributes.Final
        | MethodAttributes.Virtual
        | MethodAttributes.VtableLayoutMask;

        private static ModuleBuilder _builder;
        private static HashSet<Type> types = new HashSet<Type>();
        private static ConcurrentDictionary<string, ModuleBuilder> _moduleBuilders = new ConcurrentDictionary<string, ModuleBuilder>();
        private static ConcurrentDictionary<Type, Type> _proxyTypes = new ConcurrentDictionary<Type, Type>();

        private static ModuleBuilder GetBuilder()
        {
            if (_builder == null)
            {
                var name = "DynamicProxy";
                const AssemblyBuilderAccess access = AssemblyBuilderAccess.RunAndCollect;
#if NETCORE
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(name), access);
#else
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(name), access);
#endif
                _builder = assemblyBuilder.DefineDynamicModule(name);
            }
            return _builder;

        }
        public static Type GetType(Type type)
        {
            return _proxyTypes.GetOrAdd(type, (k) => CreateTypeFromInterface(GetBuilder(), type));
        }
        public static object CreateInstance(Type type)
        {
            if (type.IsInterface)
            {
                type = GetType(type);
            }
            var result = Activator.CreateInstance(type);
            return result;
        }
        public static object Cast(object source, Type type)
        {

            type = GetType(type);
            var result = Activator.CreateInstance(type);
            foreach (var prop in type.GetProperties())
            {
                var value = source.GetType()
                    .GetProperty(prop.Name)?
                    .GetValue(source);
                if (value != null && prop.PropertyType.IsAssignableFrom(value.GetType()))
                {
                    prop.SetValue(result, value);
                }
            }
            return result;
        }
        public static Type GetType<T>()
        {
            return GetType(typeof(T));

        }
        public static T CreateInstance<T>()
        {
            return (T)CreateInstance(typeof(T));
        }
        public static T Cast<T>(object source)
        {
            return (T)Cast(source, typeof(T));
        }
        private static Type CreateTypeFromInterface(ModuleBuilder builder, Type contract)
        {
            //var typeName = "MassTransit.DynamicContract." + (string.IsNullOrWhiteSpace(ns) ? name : $"{ns}.{name}");

            var typeName = "DynamicProxy." + contract.FullName;

            try
            {
                var typeBuilder = builder.DefineType(typeName,
                    TypeAttributes.Serializable | TypeAttributes.Class |
                    TypeAttributes.Public | TypeAttributes.Sealed,
                    typeof(object));

                typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
                if (!contract.IsInterface)
                {
                    throw new Exception("Only interfaces are supported.");
                }
                typeBuilder.AddInterfaceImplementation(contract);

                var properties = contract.GetProperties();
                foreach (var property in properties)
                {
                    var fieldBuilder = typeBuilder.DefineField("field_" + property.Name, property.PropertyType, FieldAttributes.Private);

                    var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, null);

                    var getMethod = GetGetMethodBuilder(property, typeBuilder, fieldBuilder);
                    var setMethod = GetSetMethodBuilder(property, typeBuilder, fieldBuilder);

                    propertyBuilder.SetGetMethod(getMethod);
                    propertyBuilder.SetSetMethod(setMethod);
                }
                //return typeBuilder.CreateType();
                return typeBuilder.CreateTypeInfo().AsType();
            }
            catch (Exception ex)
            {
                var message = $"Exception creating proxy ({typeName}) for {contract.Name}";

                throw new InvalidOperationException(message, ex);
            }
        }

        private static MethodBuilder GetGetMethodBuilder(PropertyInfo property, TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            var getMethodBuilder = typeBuilder.DefineMethod("get_" + property.Name,
                PropertyAccessMethodAttributes,
                property.PropertyType,
                Type.EmptyTypes);

            var il = getMethodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fieldBuilder);
            il.Emit(OpCodes.Ret);

            return getMethodBuilder;
        }

        private static MethodBuilder GetSetMethodBuilder(PropertyInfo property, TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            var setMethodBuilder = typeBuilder.DefineMethod("set_" + property.Name,
                PropertyAccessMethodAttributes,
                null,
                new[] { property.PropertyType });

            var il = setMethodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, fieldBuilder);
            il.Emit(OpCodes.Ret);

            return setMethodBuilder;
        }





    }

}
