using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Authorization
{
    public class UserLogedInModel
    {
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
    }
}
