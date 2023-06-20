using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Internals
{
    /// <summary>
    /// Handlers of this command (like ActiveDirectory) will reply with a list of 
    /// identity users. 
    /// </summary>
    public class LoadIdentitiesCommand
    {
        public int Skip { get; set; }
        public int Take { get; set; }
    }
    public class LoadIdentitiesRpply
    {
        public UserIdentityEntity[] Identities { get; set; }
    }
    public class LoadIdentityCommand
    {
        public string UserName { get; set; }
    }
    public class LoadIdentityRpply
    {
        public UserIdentityEntity Identity { get; set; }
    }

}
