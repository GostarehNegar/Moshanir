using GN.Library.Shared.Entities;
using System.DirectoryServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace GN.Library.Identity
{


    public interface IUserServices 
    {
        Task<UserEntity> AuthenticateUser(string userName, string password);
        Task<UserEntity> GetUserByToken(string token);
        Task<UserEntity> GetByUserName(string userId);
        Task<UserEntity> GetById(string id);
        Task<UserEntity> QueryUserByAttributeValue(string attributeName, string attributeValue);

    }
}
