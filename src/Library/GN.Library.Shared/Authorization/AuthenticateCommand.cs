using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Authorization
{
    public class AuthenticateCommand
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class AuthenticateResponse
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
    }
}
