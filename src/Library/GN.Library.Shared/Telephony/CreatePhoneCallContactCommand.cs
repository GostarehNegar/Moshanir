using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Internals
{
    public class CreatePhoneCallContactCommand
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string PhoneRecordId { get; set; }
    }
    public class CreatePhoneCallContactReply
    {
        public bool Success { get; set; }
        public ContactEntity Contact { get; set; }
    }
}
