using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace GN.Library.Messaging.Internals
{
    public class SubscriptionTopic
    {
        public string Subject { get; private set; }
        public string Stream { get; private set; }
        public string StreamId { get; private set; }
        public long? FromVersion { get; private set; }
        public long? ToVersion { get; private set; }
        public SubscriptionTopic AddSubject(string subject)
        {
            this.Subject = string.IsNullOrWhiteSpace(this.Subject)
                ? subject
                : $"{this.Subject},{subject}";
            return this;
        }
        public static bool WildCardMatch(string value, string pattern)
        {
            if (value == null && pattern == null)
                return true;
            if (value == null || pattern == null)
                return false;
            var exp = "^" + Regex.Escape(pattern).Replace("\\?", ".").Replace("\\*", ".*") + "$";
            return Regex.IsMatch(value, exp);
        }
        public bool Matches(MessageTopic topic)
        {
            return (this.Subject == topic.Subject || WildCardMatch(topic.Subject, this.Subject))
                && (this.Stream == topic.Stream || WildCardMatch(topic.Stream, this.Stream))
                //&& (this.StreamId == topic.StreamId || WildCardMatch(topic.Stream, this.StreamId))
                && (this.FromVersion == null || topic.Version >= this.FromVersion)
                && (this.ToVersion == null || topic.Version <= this.ToVersion);
        }
        private bool SubjectMatches(ILogicalMessage message)
        {
            return this.Subject != null && this.Subject.Split(',').Any(x => message.Subject == x || WildCardMatch(message.Subject, x));
        }
        public bool Matches(ILogicalMessage message)
        {
            return message != null && SubjectMatches(message) //  (this.Subject == message.Subject || WildCardMatch(message.Subject, this.Subject))
                && (this.Stream == message.Stream || WildCardMatch(message.Stream, this.Stream))
                //&& (this.StreamId == topic.StreamId || WildCardMatch(topic.Stream, this.StreamId))
                && (this.FromVersion == null || message.Version >= this.FromVersion)
                && (this.ToVersion == null || message.Version <= this.ToVersion);
        }


        private SubscriptionTopic(string subject, string stream, long? fromVersion, long? toVersion)
        {
            if (subject == null)
                throw new ArgumentNullException(nameof(subject));
            this.Subject = string.Join(",",
                 subject.Split(',')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToArray());
            this.Stream = stream;
            this.FromVersion = fromVersion;
            this.ToVersion = toVersion;
        }
        public override bool Equals(object obj)
        {
            if (obj is SubscriptionTopic other && other != null)
            {
                return this.Subject == other.Subject && this.Stream == other.Stream && this.FromVersion == other.FromVersion && this.ToVersion == other.ToVersion;
            }
            return base.Equals(obj);
        }
        public static SubscriptionTopic Create(string subject, string stream = null, long? fromVersion = null, long? toVersion = null)
        {
            return new SubscriptionTopic(subject, stream, fromVersion, toVersion);
        }
        public static SubscriptionTopic Create(Type type, string stream = null, long? fromVersion = null, long? toVersion = null)
        {
            return Create(MessageTopicHelper.GetTopicByType(type), stream, fromVersion, toVersion);
        }
    }
}
