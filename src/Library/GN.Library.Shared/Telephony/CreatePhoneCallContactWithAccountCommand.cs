using GN.Library.Shared.Chats;
using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Telephony
{
    public class CreatePhoneCallContactWithAccountCommand
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string PhoneRecordId { get; set; }
        public string AccountGuid { get; set; }
        public string AccountName { get; set; }
    }
    public class CreatePhoneCallContactWithAccountReply
    {
        public ContactEntity Contact { get; set; }
        public ChatAccountEntity Account { get; set; }
        public ChatPhoneCallEntity PhoneCall { get; set; }
        public bool Success { get; set; }

    }
}
