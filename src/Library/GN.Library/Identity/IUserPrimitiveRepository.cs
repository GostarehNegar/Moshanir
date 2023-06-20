using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Identity
{
    public interface IUserPrimitiveRepository
    {
        Task<UserEntity[]> LoadUsers(int skip, int take);
        Task<UserEntity> GetUserByUserId(string userName);
        Task<UserEntity> GetUserById(string id);
        Task<UserEntity> GetUserByExtension(string extension);

    }
}
