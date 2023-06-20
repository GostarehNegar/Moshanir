using GN.Library.Data.Lite;
using GN.Library.Shared.Entities;
using GN.Library.Shared.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace GN.Library.Identity
{
    public interface ILocalUserRepository : IUserPrimitiveRepository
    {
        Task Upsert(UserEntity[] user, CancellationToken cancellationToken);
        Task Upsert(UserIdentityEntity[] identities, CancellationToken cancellationToken);
        Task<UserIdentityEntity> GetIdentity(string userName, CancellationToken cancellationToken);
        Task<IEnumerable<UserEntity>> GetAllUsers(CancellationToken cancellationToken);
        Task<IEnumerable<UserIdentityEntity>> GetAllIdentities(CancellationToken cancellationToken);

    }
    class LocalUserRepository : LiteDatabaseEx, ILocalUserRepository
    {

        static string GetConnectionString()
        {
            var path = Path.Combine(Path.GetDirectoryName(typeof(LocalUserRepository).Assembly.Location), "data\\users.liteDb");
            return $"Filename={path}";

        }
        public LocalUserRepository() : base(GetConnectionString())
        {

        }

        public async Task<IEnumerable<UserIdentityEntity>> GetAllIdentities(CancellationToken cancellationToken)
        {
            using (var db = await this.Lock(false, cancellationToken))
            {
                return db.GetCollection<UserIdentityEntity>()
                    .FindAll()
                    .ToArray();
                    
            }
        }

        public async Task<IEnumerable<UserEntity>> GetAllUsers(CancellationToken cancellationToken)
        {
            using (var db = await this.Lock(false, cancellationToken))
            {
                return db.GetCollection<UserEntity>()
                    .FindAll()
                    .ToArray();
            }
        }

        public async Task<UserIdentityEntity> GetIdentity(string userName, CancellationToken cancellationToken)
        {
            using (var db = await this.Lock(false, cancellationToken))
            {
                return db.GetCollection<UserIdentityEntity>()
                    .FindById(userName?.ToLowerInvariant());
            }
        }

        public Task<UserEntity> GetUserByExtension(string extension)
        {
            throw new NotImplementedException();
        }

        public async Task<UserEntity> GetUserById(string id)
        {
            using (var db = await this.Lock(false, default))
            {
                return db.GetCollection<UserEntity>()
                    .FindById(id);

            }
        }

        public async Task<UserEntity> GetUserByUserId(string userName)
        {
            using (var db = await this.Lock(false, default))
            {
                return db.GetCollection<UserEntity>()
                    .FindOne(x => x.UserName == userName);

            }
        }

        public async Task<UserEntity[]> LoadUsers(int skip, int take)
        {
            using (var db = await this.Lock(false, default))
            {
                return db.GetCollection<UserEntity>()
                    .Find(x => true, skip, take)
                    .ToArray();

            }
        }

        public async Task Upsert(UserEntity[] user, CancellationToken cancellationToken)
        {
            using (var db = await this.Lock(true, cancellationToken))
            {

                db.GetCollection<UserEntity>()
                    .Upsert(user);
            }
        }

        public async Task Upsert(UserIdentityEntity[] identities, CancellationToken cancellationToken)
        {
            using (var db = await this.Lock(true, cancellationToken))
            {
                db.GetCollection<UserIdentityEntity>()
                    .Upsert(identities);

            }
        }
    }
}
