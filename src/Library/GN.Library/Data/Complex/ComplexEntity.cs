using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GN.Library.Data.Complex
{
	public interface IComplexEntity : IEntity
	{
		string LogicalName { get; }
	}
	public interface IComplexEntity<TKey> : IComplexEntity
	{
		TKey Id { get; }
	}
	public class ComplexEntity<TKey> : IComplexEntity<TKey>
	{
		protected object __CONTEXT__;
		public string LogicalName { get; protected set; }
		public TKey Id { get; protected set; }
	}
	
	
}
namespace GN.Library.Data
{
	using GN.Library.Data.Complex;
    

    public static partial class Extensions
	{
		private static ConcurrentDictionary<string, object> metaDataCache = new ConcurrentDictionary<string, object>();
		public static TM SendMessage<T, TM>(this IEntityContext<T> context, TM message) where T : IComplexEntity where TM : class, IComplexEntityMessage
		{
			var result = message as TM;
			if (message != null && !result.IsCompleted())
			{
				foreach (var handler in context.GetHandlers())
				{
					if (result.IsCompleted())
						break;
					result = handler(context.Target, message) as TM;
				}
			}
			return result as TM;
		}
		internal static string[] GetAttributeNames<T>(this IEntityContext<T> context) where T : IComplexEntity
		{
			var result = context.SendMessage(new EntityMessages.GetAttributeNames());
			return result.AttributeNames;
		}
		public static IComplexEntityMessage Completed(this IComplexEntityMessage message)
		{
			if (message != null)
				message.ResultCode = HandlerResultCodes.Completed;
			return message;
		}
		public static IComplexEntityMessage Unknown(this IComplexEntityMessage message)
		{
			if (message != null)
				message.ResultCode = HandlerResultCodes.Unkown;
			return message;
		}
		public static IComplexEntityMessage Fail(this IComplexEntityMessage message)
		{
			if (message != null)
				message.ResultCode = HandlerResultCodes.Failed;
			return message;
		}
		public static bool IsProcessed(this IComplexEntityMessage message)
		{
			return message == null || message.ResultCode != HandlerResultCodes.Unkown;
		}
		public static bool IsCompleted(this IComplexEntityMessage message)
		{
			return message == null || message.ResultCode == HandlerResultCodes.Completed;
		}
		internal static List<EntityMessageHandler> GetHandlers<T>(this IEntityContext<T> context) where T : IComplexEntity
		{
			return context.GetOrAddValue<List<EntityMessageHandler>>(() =>
			{
				var result = new List<EntityMessageHandler>();
				return result;
			}, "$handlerpipeline");
		}
		public static void AddHandler<T>(this IEntityContext<T> context, EntityMessageHandler handler, bool top = false) where T : IComplexEntity
		{
			if (top)
				context.GetHandlers().Insert(0, handler);
			else
				context.GetHandlers().Add(handler);
		}
		public static IComplexEntityMetaData<T> GetMetaData<T>(this IEntityContext<T> context, bool reset = false) where T : IComplexEntity
		{
			var key = "$meta";
			IComplexEntityMetaData<T> result = null;
			if (reset)
			{
				metaDataCache = new ConcurrentDictionary<string, object>();
				//metaDataCache.RemoveValue<IEntityMetaData<T>>(key);
				context.RemoveValue<IComplexEntityMetaData<T>>(key);
			}
			if (1 == 0) // metadatacache disabled.
			{
				result = context.GetOrAddValue<IComplexEntityMetaData<T>>(() =>
				{
					return new ComplexEntityMetaData<T>(context);
				}, key);
			}
			else
			{
				result = metaDataCache.GetOrAddObjectValueWithDeserialization<IComplexEntityMetaData<T>>(() => {
					return context.GetOrAddValue<IComplexEntityMetaData<T>>(() =>
					{
						return new ComplexEntityMetaData<T>(context);
					}, key);
				}, key);
			}
			return result;
		}
		public static IEntityAttributeValueCollection<T> GetAttributes<T>(this IEntityContext<T> context, bool reset = false) where T : IComplexEntity
		{
			var key = "$attributes";
			if (reset)
				context.RemoveValue<IEntityAttributeValueCollection<T>>(key);
			return context.GetOrAddValue<IEntityAttributeValueCollection<T>>(() =>
			{
				return new EntityAttributeValueCollection<T>(context);
			}, key);
		}
		internal static KeyValuePair<string, object>[] GetSampleData<T>(this IEntityContext<T> context, KeyValuePair<string, object>[] sample) where T : IComplexEntity
		{
			return context.GetOrAddValue<KeyValuePair<string, object>[]>(() => sample, "$sampledata");
		}
		internal static ConcurrentDictionary<string, object> GetData<T>(this IEntityContext<T> context) where T : IComplexEntity
		{
			return context.GetOrAddValue<ConcurrentDictionary<string, object>>(() => new ConcurrentDictionary<string, object>(), "$data"); ;
		}
		public static IComplexEntityMessage DefaultConversionHandler(IComplexEntity entity, IComplexEntityMessage message)
		{
			var _message = message as EntityMessages.Convert;
			if (_message != null)
			{
				var value = _message?.SourceValue?.GetRawValue();
				var targetType = _message?.TargetType;
				if (value != null && targetType != null)
				{
					if (targetType.IsAssignableFrom(value.GetType()))
					{
						_message.ResultValue = value;
						return _message.Completed();
					}
					if (targetType == typeof(string))
					{

					}


				}
				else if (value == null)
				{
					_message.ResultValue = null;
					_message.Completed();
				}
				else
				{
					_message.Fail();
					_message.ResultValue = null;
				}

			}
			else
			{
				//message.Unknown();
			}
			return message;
		}
		private static IComplexEntityMessage GenericHandler(IComplexEntity entity, IComplexEntityMessage message)
		{
			var sample = entity.GetContext().GetSampleData(new KeyValuePair<string, object>[] { });
			var data = entity.GetContext().GetData();

			if (message != null)
			{
				switch (message)
				{
					case EntityMessages.GetAttributeNames m:
						m.AttributeNames = sample.Select(x => x.Key).ToArray();
						m.Completed();
						break;
					case EntityMessages.GetRawValue m:
						if (data.TryGetValue(m.AttributeName, out var tmp))
							m.Value = tmp;
						return m.Completed();
					case EntityMessages.SetRawValue m:
						data.AddOrUpdate(m.AttributeName, m.Value, (k, o) => m.Value);
						return m.Completed();
					case EntityMessages.GetAttributeMetaData m:
						var _data = sample.FirstOrDefault(x => x.Key == m.AttributeName);
						if (_data.Value != null)
						{
							m.RawType = _data.Value.GetType();
						}
						return m.Completed();
					case EntityMessages.Convert m:

						if (TryConvert(m.SourceValue.GetRawValue(), m.TargetType, out var _tmp))
						{
							m.ResultValue = _tmp;
							return m.Completed();
						}
						break;
					default:
						break;

				}
			}

			return message;
		}
		public static void InitializeGenericEntity<T>(this IEntityContext<T> context, KeyValuePair<string, object>[] sampleData) where T : IComplexEntity
		{
			context.GetSampleData(sampleData);
			context.AddHandler(GenericHandler);
			context.AddHandler(DefaultConversionHandler);
		}
		public static IComplexEntityAttributeValue GetAttribute<T>(this T entity, string attributeName, bool refersh = false) where T : IComplexEntity
		{
			return entity.GetContext().GetAttributes(refersh).GetByName(attributeName);
		}
		internal static bool TryConvert(object source, Type type, out object result)
		{
			return AppHost.Utils.InternalTryConvert(source, type, out result);
		}
		internal static bool IsNullable(this Type type)
		{
			return type == null || !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
		}
	}

}
