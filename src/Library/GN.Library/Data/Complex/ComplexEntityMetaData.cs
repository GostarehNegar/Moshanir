using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace GN.Library.Data.Complex
{
	public interface IComplexEntityMetaData<T> where T : IComplexEntity
	{
		IComplexEntityAttributeMetaData GetAttribute(string attributeName, bool refersh = false, bool throwIfNotFound = true);
		string[] GetAttributeNames(bool refersh = false);
		bool EnsureAttributeExists(string attributeName, bool throwIfNotFound = true);
	}
	public interface IComplexEntityAttributeMetaData
	{
		string Name { get; }
		Type GetType(bool refersh = false);
		object CustomData { get; }
	}
	internal class ComplexEntityAttributeMetaData<T> : IComplexEntityAttributeMetaData where T : IComplexEntity
	{
		protected object __CONTEXT__;
		public ComplexEntityMetaData<T> Parent { get; private set; }
		public string Name { get; private set; }

		public object CustomData => GetMetaMessage()?.CustomData;

		private EntityMessages.GetAttributeMetaData metaMessage;
		public Type GetType(bool refersh = false)
		{
			var msg = this.GetMetaMessage(refersh);
			return msg.IsCompleted() ? msg.RawType : null;
		}
		private EntityMessages.GetAttributeMetaData GetMetaMessage(bool refersh = false)
		{
			if (metaMessage == null || refersh)
			{
				metaMessage = this.Parent.Context.SendMessage(new EntityMessages.GetAttributeMetaData(this.Name));
			}
			return this.metaMessage;
		}
		public ComplexEntityAttributeMetaData(ComplexEntityMetaData<T> parent, string name)
		{
			this.Parent = parent;
			this.Name = name;
		}

	}
	internal class ComplexEntityMetaData<T> : IComplexEntityMetaData<T> where T : IComplexEntity
	{
		protected object __CONTEXT__;
		private ConcurrentDictionary<string, ComplexEntityAttributeMetaData<T>> items = new ConcurrentDictionary<string, ComplexEntityAttributeMetaData<T>>();

		private string[] attributeNames;
		public IEntityContext<T> Context { get; private set; }

		public ConcurrentDictionary<string, ComplexEntityAttributeMetaData<T>> Items
		{
			get
			{
				this.items = this.items ?? new ConcurrentDictionary<string, ComplexEntityAttributeMetaData<T>>();
				return this.items;
			}
		}


		public IComplexEntityAttributeMetaData GetAttribute(string attributeName, bool refresh = false, bool throwIfNotFound = true)
		{
			ComplexEntityAttributeMetaData<T> result = null;
			if (refresh)
				this.Items.TryRemove(attributeName, out var tmp);
			if (this.EnsureAttributeExists(attributeName, throwIfNotFound))
			{
				result = this.Items.GetOrAdd(attributeName, k =>
				{
					return new ComplexEntityAttributeMetaData<T>(this, attributeName);
				});
			}
			return result;
		}

		public string[] GetAttributeNames(bool refersh = false)
		{
			if (this.attributeNames == null || refersh)
			{
				this.attributeNames = this.Context.GetAttributeNames();
			}
			return this.attributeNames;
		}

		public bool EnsureAttributeExists(string attributeName, bool throwIfNotFound = true)
		{
			var result = this.GetAttributeNames().Contains(attributeName);
			if (!result && throwIfNotFound)
			{
				throw new Exception($"Attribute not found :{attributeName}");
			}
			return result;
		}

		public ComplexEntityMetaData(IEntityContext<T> context)
		{
			this.Context = context;

		}
	}

}
