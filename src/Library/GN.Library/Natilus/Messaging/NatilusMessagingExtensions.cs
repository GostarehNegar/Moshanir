using GN.Library.Natilus.Messaging.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client.JetStream;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace GN.Library.Natilus.Messaging
{
    public static class NatilusMessagingExtensions
    {
        private static MethodInfo _GetLookupStreamBySubjectMethod;
        internal static MethodInfo GetLookupStreamBySubjectMethod(this IJetStream jet)
        {
            if (_GetLookupStreamBySubjectMethod == null)
            {
                _GetLookupStreamBySubjectMethod = jet
                    .GetType()
                    .GetMethod("LookupStreamBySubject", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            return _GetLookupStreamBySubjectMethod;
        }
    }
}
