using GN.Library.Shared.Entities;
using GN.Library.Shared.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Identity
{
    //public interface IUserRepository
    //{
    //    Task<UserEntity[]> LoadUsers(int skip, int take);
    //}
    public interface IIdentityServices : IAuthenticationProvider
    {
        Task<IQueryable<UserEntity>> GetQueryable();
        Task<IQueryable<UserIdentityEntity>> GetIdentities();

    }
}
