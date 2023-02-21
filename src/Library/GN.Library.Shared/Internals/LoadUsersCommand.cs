using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Internals
{
    public class LoadUsersCommand
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public LoadUsersCommand Validate()
        {
            Take = Take < 10 ? 10 : Take;
            Take = Take > 1000 ? 1000 : Take;
            return this;
        }
    }
    public class LoadUsersReply
    {
        public UserEntity[] Users { get; set; }
    }
}
