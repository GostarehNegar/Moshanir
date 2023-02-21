using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Data
{
	public interface IDynamicEntity : IEntity
	{
		IAttributeCollection Attributes { get; }
		string LogicalName { get; }
		object Id { get; set; }
		IDynamicEntity init(object id, string logicalName, IDictionary<string, object> attributes);
	}
	public interface IDynamicEntity<TKey> : IDynamicEntity, IEntity<TKey>
	{
		IDynamicEntity<TKey> init(TKey id, string logicalName, IDictionary<string, object> attributes);
	}
	public interface IDynamicEntity<TKey, T> : IDynamicEntity<TKey>
	{
	}
	public interface IAttributeCollection : IDictionary<string, object> { }
	

	class AttributeCollection: ConcurrentDictionary<string,object>, IAttributeCollection
	{
		public AttributeCollection()
		{
		}
		public AttributeCollection(IDictionary<string,object> vals) : base(vals)
		{
			
		}
	}
	public class DynamicEntity : IDynamicEntity
	{
		protected IAttributeCollection attributes = new AttributeCollection();
		public string LogicalName { get; protected set; }
		public IAttributeCollection Attributes => attributes;
		public object Id { get;  set; }
		public virtual IDynamicEntity init(object id, string logicalName, IDictionary<string, object> attributes)
		{
			this.Id = id;
			this.LogicalName = logicalName;
			this.attributes = new AttributeCollection(attributes ?? new Dictionary<string, object>());
			return this;
		}
	}
	public class DynamicEntity<TKey> : DynamicEntity, IDynamicEntity<TKey>
	{
		protected TKey GetId()
		{
			return (TKey)base.Id;
		}
		public new TKey Id { get => this.GetId(); set => base.Id = value; }
		public virtual IDynamicEntity<TKey> init(TKey id, string logicalName, IDictionary<string, object> attributes)
		{
			base.Id = id;
			this.LogicalName = logicalName;
			this.attributes = new AttributeCollection(attributes ?? new Dictionary<string, object>());
			return this;
		}

	}
	public class DynamicEntity<TKey, T> : DynamicEntity<TKey>, IDynamicEntity<TKey, T>
	{

	}

}
