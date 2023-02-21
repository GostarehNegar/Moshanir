using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Data.Complex
{
	public delegate IComplexEntityMessage EntityMessageHandler(IComplexEntity entity, IComplexEntityMessage message);

	public enum HandlerResultCodes
	{
		Unkown = 0,
		Failed = -1,
		Processed = 1,
		Completed = 2,
	}
	public interface IComplexEntityMessage
	{
		string Message { get; }
		HandlerResultCodes ResultCode { get; set; }
	}
	public static class EntityMessages
	{
		public class BaseMessage : IComplexEntityMessage
		{
			protected object __CONTEXT__;
			public virtual string Message => this.GetType().Name;
			public HandlerResultCodes ResultCode { get; set; }
		}
		public class GetRawValue : BaseMessage //IEntityMessage
		{
			public string AttributeName { get; }
			public object Value { get; set; }

			public GetRawValue(string attributeName)
			{
				this.AttributeName = attributeName;
			}
		}
		public class SetRawValue : BaseMessage
		{
			public string AttributeName { get; private set; }
			public object Value { get; private set; }
			public SetRawValue(string attributeName, object value)
			{
				this.AttributeName = attributeName;
				this.Value = value;
			}
		}
		public class GetAttributeNames : BaseMessage
		{
			public string[] AttributeNames { get; set; }
			public GetAttributeNames()
			{
			}
		}
		public class GetAttributeMetaData : BaseMessage
		{
			public string AttributeName { get; private set; }
			public Type RawType { get; set; }
			public Type RawSetType { get; set; }
			public object CustomData { get; set; }
			public GetAttributeMetaData(string name)
			{
				this.AttributeName = name;
			}
		}
		public class Convert : BaseMessage
		{
			public enum ConversionOperationType
			{
				Get,
				Set,
				Serialize,
				Deserialize,
			}
			public IComplexEntityAttributeValue SourceValue { get; }
			public object ResultValue { get; set; }
			public Type TargetType { get; }
			public ConversionOperationType Operation { get; }

			public Convert(IComplexEntityAttributeValue source, Type targetType, ConversionOperationType operation)
			{
				this.SourceValue = source;
				this.TargetType = targetType;
				this.Operation = operation;
			}
			public T GetResult<T>()
			{
				var result = ResultValue != null && typeof(T).IsAssignableFrom(ResultValue.GetType())
					? (T)ResultValue
					: default(T);
				return result;
			}

		}


	}
}
