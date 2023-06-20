using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Identity
{
    public class AuthenticateUserRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class AuthenticateUserResponse
    {
        public UserEntity User { get; set; }
    }
    public class QueryUserRequest
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string TelephoneExtension { get; set; }

        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        
    }
    public class QueryUserResponse
    {
        public UserEntity User { get; set; }
    }
}
