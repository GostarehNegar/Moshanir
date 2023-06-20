using GN.Library.Messaging;
using GN.Library.Shared;
using GN.Library.Shared.Entities;
using GN.Library.Shared.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Identity
{
    class UserServices : IUserServices
    {
        private readonly IServiceProvider serviceProvider;
        private LocalUserServices local;
        private IProcedureCall rpc;
        public UserServices(IServiceProvider serviceProvider)
        {
            this.local = serviceProvider.GetServiceEx<LocalUserServices>();
            this.serviceProvider = serviceProvider;
            this.rpc = this.serviceProvider.GetServiceEx<IMessageBus>().Rpc;
        }
        public async Task<UserEntity> AuthenticateUser(string userName, string password)
        {
            return this.local != null
                ? await this.local.AuthenticateUser(userName, password)
                : (await this.rpc.Call<AuthenticateUserRequest, AuthenticateUserResponse>(new AuthenticateUserRequest
                {
                    UserName = userName,
                    Password = password
                })).User;


        }

        public async Task<UserEntity> GetById(string id)
        {
            return this.local != null
               ? await this.local.GetById(id)
               : (await this.rpc.Call<QueryUserRequest, QueryUserResponse>(new QueryUserRequest { UserId = id }))
                ?.User;
        }

        public async Task<UserEntity> GetByUserName(string userId)
        {
            return this.local != null
               ? await this.local.GetByUserName(userId)
               : (await this.rpc.Call<QueryUserRequest, QueryUserResponse>(new QueryUserRequest { UserName = userId }))
                ?.User;

        }

        public async Task<UserEntity> GetUserByToken(string token)
        {
            return this.local != null
               ? await this.local.GetUserByToken(token)
               : (await this.rpc.Call<QueryUserRequest, QueryUserResponse>(new QueryUserRequest { Token = token }))
                ?.User;
        }

        public async Task<UserEntity> QueryUserByAttributeValue(string attributeName, string attributeValue)
        {
            return this.local != null
              ? await this.local.QueryUserByAttributeValue(attributeName, attributeValue)
              : (await this.rpc.Call<QueryUserRequest, QueryUserResponse>(new QueryUserRequest { AttributeName = attributeValue, AttributeValue = attributeValue }))
               ?.User;
        }
    }
}
