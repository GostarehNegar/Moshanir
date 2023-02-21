
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Data
{

	public interface IEntity
	{
	}
	public interface IEntity<TKey> : IEntity
	{
		TKey Id { get;  }
		string LogicalName { get; }

	}
	public interface IGenericEntity<TKey>
	{
		TKey Id { get; set; }
	}
	public interface IGenericEntityEx<TKey> : IGenericEntity<TKey>
	{
		IDictionary<string, object> Attributes { get; }
		IDictionary<string, object> Slots { get; }
		string LogicalName { get; }
	}
	public interface IGenericEntityEx : IGenericEntityEx<Guid> { }


	//	public interface IEntityEx<TKey>
	//	{
	//		string LogicalName { get; }
	//		TKey Id { get; }
	//		EntitySchema GetSchema(bool refersh = false);
	//		IContext<IEntityEx<TKey>> Context();
	//		T GetOrAddAttributeValue<T>(string attributeName, Func<IEntityEx<TKey>, KeyValuePair<string, object>, T> evaluator);
	//		bool TryGetOrAddAttributeValue<T>(string attributeName, out T result, Func<IEntityEx<TKey>, KeyValuePair<string, object>, T> resolver);
	//		void AddOrUpdateAttibuteValue<T>(string attributeName, Func<IEntityEx<TKey>, KeyValuePair<string, object>, T> resolver);
	//		bool HasAttributeValue(string attibuteName);
	//		void RemoveAttributeValue(string attributeName);
	//		void init(TKey key = default(TKey), string logicalName = null, IDictionary<string, object> attributes = null);
	//	}

	//	public interface IRecord { }
	//	public interface IRecord<TKey> : IRecord
	//	{
	//		string LogicalName { get; }
	//		TKey Id { get; }
	//	}
	//	public class Record<TKey> : IRecord<TKey>
	//	{
	//		protected object __CONTEXT__;
	//		public string LogicalName { get; protected set; }
	//		public TKey Id { get; protected set; }
	//		public Record(string logicalName, TKey id)
	//		{
	//			this.LogicalName = logicalName;
	//			this.Id = id;

	//		}

	//	}
	//	public interface IMetaDataProvider<T> where T : IRecord
	//	{
	//		EntityAttributeDefinition GetAttributeMetaData(string attributeName);
	//	}
	//	public class EntityAttributeDefinition
	//	{
	//		public string Name { get; private set; }
	//		public Type Type { get; private set; }
	//		public T Convert<T>(object value)
	//		{
	//			throw new NotImplementedException();
	//		}
	//	}
	//	public class EntitySchema
	//	{
	//		private Dictionary<string, EntityAttributeDefinition> attributes;
	//		public string LogicalName { get; private set; }
	//		public IReadOnlyDictionary<string, EntityAttributeDefinition> Attributes => this.attributes;
	//		public EntitySchema(string logicalName, IDictionary<string, EntityAttributeDefinition> schema)
	//		{
	//			this.LogicalName = logicalName;
	//			this.attributes = new Dictionary<string, EntityAttributeDefinition>(schema);
	//		}
	//	}
	//	public class AttributeValue
	//	{
	//		object value { get; set; }

	//	}
	//	public interface IEntityAttributeConverter
	//	{
	//		T Convert<T>(object value, EntityAttributeDefinition schema);
	//	}
	//	public class EntityEx<TKey> : IEntityEx<TKey>
	//	{
	//		protected object __CONTEXT__;
	//		protected ConcurrentDictionary<string, object> attributes;
	//		protected string logicalName;
	//		protected TKey id;
	//		protected EntitySchema schema;
	//		public IDictionary<string, object> Attributes => this.attributes;
	//		public string LogicalName => this.logicalName;
	//		public TKey Id => this.id;
	//		public EntityEx(TKey id, string logicalName, IDictionary<string, object> attributes, IDictionary<string, object> slots)
	//		{
	//			this.id = id;
	//			this.logicalName = logicalName;
	//			this.attributes = new ConcurrentDictionary<string, object>(attributes ?? new Dictionary<string, object>());
	//			this.GetContext(slots);
	//			//this.slots = new ConcurrentDictionary<string, object>(slots ?? new Dictionary<string, object>());
	//		}
	//		public T GetAttributeValue<T>(string key)
	//		{
	//			throw new NotImplementedException();
	//		}
	//		public void SetArributeValue<T>(string key, T value)
	//		{
	//			throw new NotImplementedException();
	//		}
	//		protected virtual EntitySchema GetSchema(bool refersh = false)
	//		{
	//			return null;
	//		}
	//		public IContext<IEntityEx<TKey>> Context()
	//		{
	//			return this.GetContext();
	//		}
	//		EntitySchema IEntityEx<TKey>.GetSchema(bool refersh)
	//		{

	//			if (this.schema == null || refersh)
	//			{
	//				this.schema = this.GetSchema();
	//			}
	//			return this.schema;
	//		}
	//		public T GetOrAddAttributeValue<T>(string attributeName, Func<IEntityEx<TKey>, KeyValuePair<string, object>, T> evaluator)
	//		{
	//			throw new NotImplementedException();
	//		}
	//		public bool TryGetOrAddAttributeValue<T>(string attributeName, out T result, Func<IEntityEx<TKey>, KeyValuePair<string, object>, T> resolver)
	//		{
	//			throw new NotImplementedException();
	//		}
	//		public void AddOrUpdateAttibuteValue<T>(string attributeName, Func<IEntityEx<TKey>, KeyValuePair<string, object>, T> resolver)
	//		{
	//			throw new NotImplementedException();
	//		}
	//		public bool HasAttributeValue(string attibuteName)
	//		{
	//			throw new NotImplementedException();
	//		}
	//		public void RemoveAttributeValue(string attributeName)
	//		{
	//			throw new NotImplementedException();
	//		}
	//		public void init(TKey key = default, string logicalName = null, IDictionary<string, object> attributes = null)
	//		{
	//			throw new NotImplementedException();
	//		}
	//	}
	//	public static class DataEntityExtensions
	//	{
	//		public static ConcurrentDictionary<string, object> GetAttributes(this IContext<IRecord> record)
	//		{
	//			return record.GetOrAddValue(() => new ConcurrentDictionary<string, object>(), "$attributes");
	//		}

	//		public static void Load(this IContext<IRecord> record, IDictionary<string,object> values )
	//		{

	//		}
	//		public static T GetAttributeValue<T>(this IRecord record, string attributeName)
	//		{
	//			var meta = record.GetMetaData()?.GetAttributeMetaData(attributeName);
	//			record.GetContext().GetAttributes().TryGetValue(attributeName, out var o);
	//			return meta.Convert<T>(o);



	//		}

	//		public static IMetaDataProvider<T> GetMetaData<T>(this T record, IMetaDataProvider<T> value = null) where T : IRecord
	//		{
	//			return null;
	//		}
	//		public static void RegisterLoader<T>(this T record, Func<T,IDictionary<string,object>> valueFunc)
	//		{

	//		}
	//	}
}
