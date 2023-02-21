using NATS.Client;
using NATS.Client.JetStream;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Natilus.Messaging.Internals
{
    class JetStreamHelper
    {
        private readonly IConnection connection;
        private IJetStream jetStream;
        private ConcurrentDictionary<string, string> subjectStreamMap = new ConcurrentDictionary<string, string>();

        public JetStreamHelper(IConnection connection)
        {
            this.connection = connection;
        }
        public bool SubjectHasStream(string subject)
        {
            return !string.IsNullOrWhiteSpace(this.GetStreamNameBySubject(subject));
        }
        public bool ConnectionSupportsJetStream()
        {
            return true;
        }

        public IJetStream GetJetStream(bool refresh = false)
        {
            if (this.jetStream == null || refresh)
            {
                this.jetStream = this.connection.CreateJetStreamContext();
            }
            return this.jetStream;
        }
        public string GetStreamNameBySubject(string subject)
        {
            return this.subjectStreamMap.GetOrAdd(subject, s =>
            {
                var method = this.GetJetStream().GetLookupStreamBySubjectMethod();// this.GetLookupStreamBySubjectMethod();
                var streamName = (string)method.Invoke(this.GetJetStream(), new object[] { subject });
                return streamName;

            });
        }
    }
}
