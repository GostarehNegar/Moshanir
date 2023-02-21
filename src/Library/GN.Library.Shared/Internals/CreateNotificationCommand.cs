using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Internals
{
    public class CreateNotificationCommand
    {
        public string Subject { get; set; }
        public string URL { get; set; }
        public string UserGuid { get; set; }
    }
    public class CreateNotificationReply
    {
        public bool Success { get; set; }
    }

    public class InitiateNotificationCommand : CreateNotificationCommand
    {
        public bool SendWithoutCreation => false;
    }
    public class InitiateNotificationReply : CreateNotificationReply { }

    public class SendPushNotificationCommandEx : CreateNotificationCommand { }
    public class SendPushNotificationReplyEx : CreateNotificationReply { }
}


