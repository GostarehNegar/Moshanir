using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Telephony
{
    public class UpdatePhoneCallSubjectCommand
    {
        public string Id { get; set; }
        public string Subject { get; set; }
    }
    public class UpdatePhoneCallSubjectReply
    {
        public bool Success { get; set; }
    }
}
