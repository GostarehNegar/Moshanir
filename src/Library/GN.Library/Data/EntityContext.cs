using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Dynamic;
using System.Text;

namespace GN.Library.Data
{
	/*
	 * 
	 */

	/// <summary>
	/// A generic object context, that provides a dictionary of
	/// local properties.
	/// </summary>
	public interface IEntityContext
	{
		object Target { get; }
	}
	/// <summary>
	/// A targeted object context that provides a dictionary of
	/// properties together with a target object.
	/// To use it one should simply provide a protected object
	/// field named '__CONTEXT__':
	///		protected object __CONTEXT__
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IEntityContext<out T> : IEntityContext
	{
		new T Target { get; }
	}
	internal class EntityContext : IEntityContext
	{
		protected ConcurrentDictionary<string, object> slots = new ConcurrentDictionary<string, object>();
		public object Target { get; private set; }
		public IDictionary<string, object> Slots => this.slots;
		public IServiceProvider ServiceProvider => AppHost.Services.Provider;
		internal EntityContext(object target, IDictionary<string, object> slots = null)
		{
			this.Target = target;
			this.slots = new ConcurrentDictionary<string, object>(slots ?? new Dictionary<string, object>());
		}

	}
	internal class EntityContext<T> : EntityContext, IEntityContext<T>
	{
		private ConcurrentDictionary<string, object> slots = new ConcurrentDictionary<string, object>();
		public new T Target { get; private set; }
		internal EntityContext(T target, IDictionary<string, object> slots = null) : base(target, slots)
		{
			this.Target = target;
		}
	}

	internal class AppContextBase
	{

		private ConcurrentDictionary<string, object> props = new ConcurrentDictionary<string, object>();
		public IDictionary<string, object> Props => this.props;

		public IAppContext Context { get; private set; }
		public AppContextBase(IAppContext context)
		{
			this.Context = context;
		}
	}
	public static partial class Extensions
	{
		/// <summary>
		/// Returns a generic object context. This context provides a
		/// dictionary of properties together with access to an instance of
		/// IServiceProvider.
		/// The target object should have an object field named '__CONTEXT__'.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="target"></param>
		/// <param name="slots"></param>
		/// <param name="ThrowContextNotSupported"></param>
		/// <param name="reset"></param>
		/// <returns></returns>
		public static IEntityContext<T> GetContext<T>(this T target, IDictionary<string, object> slots = null, bool ThrowContextNotSupported = false, bool reset = false)
		{
			IEntityContext<T> result = null;
			var field = target?.GetType()
				.GetField("__CONTEXT__", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			if (field == null && ThrowContextNotSupported)
				throw new Exception($"Context is not supported in this type '{typeof(T).FullName}'. Supported Type should have '__CONTEXT__' field.");
			result = field?.GetValue(target) as IEntityContext<T>;
			if (result as IEntityContext<T> == null || reset)
			{
				result = new EntityContext<T>(target, slots);
				field?.SetValue(target, result);
			}
			return result;
		}
		private static EntityContext Ensure(IEntityContext context)
		{
			var result = context as EntityContext;
			if (result == null)
				throw new Exception("Invalid Context.");
			return result;
		}
		public static T GetOrAddValue<T>(this IEntityContext context, Func<T> constructor = null, string key = null)
		{
			return Ensure(context).Slots.GetOrAddObjectValueWithDeserialization<T>(constructor, key);
		}
		public static T AddOrUpdateValue<T>(this IEntityContext context, Func<T> constructor, string key = null)
		{
			return Ensure(context).Slots.AddOrUpdateObjectValue(constructor, key);
		}
		public static void RemoveValue<T>(this IEntityContext context, string key = null)
		{
			Ensure(context).Slots.RemoveObjectValue<T>(key);
		}
		public static bool TryGetValue<T>(this IEntityContext context, out T value, string key = null)
		{
			return Ensure(context).Slots.TryGetObjectValue<T>(out value, key);

		}
		public static IServiceProvider ServiceProvider(this IEntityContext context)
		{
			return Ensure(context).ServiceProvider;
		}
		public static IDictionary<string, object> Slots(this IEntityContext context)
		{
			return Ensure(context).Slots;
		}


	}
}
