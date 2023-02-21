using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Data.Complex
{
	public enum ComplexAttributeValueStatus
	{
		Unknown = 0,
		Changed = 1
	}
	public interface IComplexEntityAttributeValue
	{

		T Get<T>(bool throwIfFailed = true, bool refersh = false);
		void Set<TV>(TV value);
		IComplexEntityAttributeMetaData GetMetaData(bool refersh = false);
		ComplexAttributeValueStatus Status { get; }
		object GetRawValue(bool referesh = false);


	}
	public interface IComplexEntityAttributeValue<T> : IComplexEntityAttributeValue where T : IComplexEntity
	{

	}

	public interface IEntityAttributeValueCollection<T> where T : IComplexEntity
	{
		IComplexEntityAttributeValue GetByName(string attributeName, bool refresh = false, bool throwIfNotFound = true);
		
	}

	internal class EntityAttributeValue<T> : IComplexEntityAttributeValue<T> where T : IComplexEntity
	{
		protected object __CONTEXT__;
		public EntityAttributeValueCollection<T> Parent { get; private set; }

		private ConcurrentDictionary<string, object> values = new ConcurrentDictionary<string, object>();

		public ComplexAttributeValueStatus Status { get; private set; }
		public string Name { get; private set; }

		private object rawValue;
		private bool _rawValue;

		public object GetRawValue(bool refersh = false)
		{
			if (!_rawValue || refersh)
			{
				var message = this.Parent.Context.SendMessage(new EntityMessages.GetRawValue(this.Name));
				if (message.IsCompleted())
				{
					rawValue = message.Value;
					_rawValue = true;
				}
				else
				{
					// TODO Throw Exception?
					throw new Exception($"GetRawValue Failed for attribute:{this.Name}. Entity:{typeof(T).Name}");
				}
			}
			return this.rawValue;
		}

		public bool TryGetValue<TV>(out TV value, bool refersh = false)
		{
			value = default(TV);
			var result = false;
			var rawValue = this.GetRawValue(refersh);
			if (rawValue != null)
			{
				if (typeof(TV).IsAssignableFrom(rawValue.GetType()))
				{
					value = (TV)rawValue;
					result = true;
				}
				else
				{
					var msg = this.Parent.Context.SendMessage(new EntityMessages.Convert(this, typeof(TV), EntityMessages.Convert.ConversionOperationType.Get));
					if (msg.IsCompleted())
					{
						value = msg.GetResult<TV>();
						result = true;
					}
				}
			}
			else if (typeof(TV).IsNullable())
			{
				result = true;
			}
			return result;

		}
		public T1 DoGet<T1>(bool throwIfFailed, bool refersh)
		{
			T1 result = default(T1);
			var rawValue = this.GetRawValue(refersh);
			if (rawValue != null)
			{
				if (typeof(T1).IsAssignableFrom(rawValue.GetType()))
				{
					result = (T1)rawValue;
				}
				else
				{
					var msg = this.Parent.Context.SendMessage(new EntityMessages.Convert(this, typeof(T1), EntityMessages.Convert.ConversionOperationType.Get));
					if (msg.IsCompleted())
					{
						result = msg.GetResult<T1>();
					}
					else if (throwIfFailed)
					{
						throw new Exception($"Conversion Error. Failed to convert from '{rawValue.GetType()}' to '{typeof(T1).Name}'");
					}
				}
			}
			else if (!typeof(T1).IsNullable())
			{
				if (throwIfFailed)
				{
					throw new Exception($"Null value for not nullable type {typeof(T1).Name}");
				}
			}
			return result;
		}
		public T1 Get<T1>(bool throwIfFailed = true, bool refersh = false)
		{
			this.values = this.values ?? new ConcurrentDictionary<string, object>();
			if (refersh)
				this.values.RemoveObjectValue<T1>("$value");
			return this.values.GetOrAddObjectValueWithDeserialization<T1>(() =>
			{

				if (this.TryGetValue<T1>(out var tmp, refersh))
				{
					return tmp;
				}
				else if (throwIfFailed)
				{
					throw new Exception(
						$"Cast Error. Failed to convert '{this.GetRawValue()}' from type '{this.GetRawValue()?.GetType().Name}' to type '{typeof(T1).Name}'");


				}
				return default(T1);

			}, "$value");

			return this.values.GetOrAddObjectValueWithDeserialization<T1>(() => this.DoGet<T1>(throwIfFailed, refersh), "$value");


		}

		public IComplexEntityAttributeMetaData GetMetaData(bool refersh = false)
		{
			return this.Parent.Context.GetMetaData(refersh).GetAttribute(this.Name);
		}

		public void Set<TV>(TV value)
		{
			this.Parent.Context.SendMessage(new EntityMessages.SetRawValue(this.Name, value));
			this.values = new ConcurrentDictionary<string, object>();
			this.GetRawValue(true);
			this.Status = ComplexAttributeValueStatus.Changed;
		}

		public EntityAttributeValue(EntityAttributeValueCollection<T> parent, string name)
		{
			this.Parent = parent;
			this.Name = name;
		}

	}
	internal class EntityAttributeValueCollection<T> : IEntityAttributeValueCollection<T> where T : IComplexEntity
	{
		protected object __CONTEXT__;
		private ConcurrentDictionary<string, EntityAttributeValue<T>> items;
		public IEntityContext<T> Context { get; private set; }
		public ConcurrentDictionary<string, EntityAttributeValue<T>> GetItems(bool refersh = false)
		{
			if (this.items == null || refersh)
			{
				this.items = new ConcurrentDictionary<string, EntityAttributeValue<T>>();
			}
			return this.items;
		}
		public ConcurrentDictionary<string, EntityAttributeValue<T>> Items => this.GetItems();
		public IComplexEntityAttributeValue GetByName(string attributeName, bool refresh = false, bool throwIfNotFound = true)
		{
			EntityAttributeValue<T> result = null;
			if (refresh)
				this.items.TryRemove(attributeName, out var tmp);
			if (!this.Items.TryGetValue(attributeName, out result) && this.Context.GetMetaData().EnsureAttributeExists(attributeName, throwIfNotFound))
			{
				result = new EntityAttributeValue<T>(this, attributeName);
				this.Items.TryAdd(attributeName, result);
			}
			return result;
		}

		public EntityAttributeValueCollection(IEntityContext<T> context)
		{
			this.Context = context;
		}
	}
}
