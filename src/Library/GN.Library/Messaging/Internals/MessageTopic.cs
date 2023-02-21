using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Messaging.Internals
{
	/// <summary>
	/// Represents a message topic that consists of a 
	/// Subject and Stream.
	/// </summary>
	public class MessageTopic
	{
		public string Subject { get; private set; }
		public string Stream { get; private set; }
		public long? Version { get; private set; }
		public MessageTopic(string subject, string stream,  long? version)
		{
			this.Subject = subject;
			this.Stream = stream;
			this.Version = version;
		}
		public static MessageTopic Create(string subject, string stream = null,  long? version = null)
		{
			return new MessageTopic(subject, stream,  version);
		}
		public static MessageTopic Create(Type eventType, string stream = null,  long? version = null)
		{
			return Create(MessageTopicHelper.GetTopicByType(eventType), stream,  version);
		}
		public static implicit operator MessageTopic(string subject) => MessageTopic.Create(subject);
		public static implicit operator MessageTopic(Type subject) => MessageTopic.Create(subject);
		public void SetVersion (long? version)
        {
			this.Version = version;
        }
	}
}
