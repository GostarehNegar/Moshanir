using GN.Library.Shared.Chats;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Telephony
{
    public class AddExistingContactToPhoneCallCommand
    {
        public string PhoneRecordId { get; set; }
        public string ContactId { get; set; }
    }
    public class AddExistingContactToPhoneCallReply
    {
        public bool Success { get; set; }
        public ChatPhoneCallEntity PhoneCall { get; set; }
    }
}
