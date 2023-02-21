using GN.Library.Helpers;
using GN.Library.Identity;
using GN.Library.Messaging;
using GN.Library.Shared.Entities;
using GN.Library.Shared.Internals;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static GN.Library.LibraryConstants;

namespace GN.Library.Services
{


    public interface IUserIdentityServices : IIdentityServices, IHostedService
    {
        //Task<IQueryable<UserEntity>> GetQueryable();
        //Task<bool> Authenticate(string userName, string password);
        Task<UserEntity> AuthenticateUser(string userName, string password);
        Task<UserEntity> GetUserByToken(string token);

    }
    class UserServices : BackgroundService, IUserIdentityServices
    {
        private readonly IMessageBus bus;
        private readonly ILogger<UserServices> logger;
        private readonly IMemoryCache cache;
        private readonly ITokenService tokenService;
        private readonly IServiceProvider serviceProvider;

        private UserEntity[] users;

        public UserServices(IMessageBus bus,
                            ILogger<UserServices> logger,
                            IMemoryCache cache,
                            ITokenService tokenService, IServiceProvider serviceProvider)
        {
            this.bus = bus;
            this.logger = logger;
            this.cache = cache;
            this.tokenService = tokenService;
            this.serviceProvider = serviceProvider;
        }
        public async Task<bool> Authenticate(string userName, string password)
        {
            try
            {
                var service = this.serviceProvider.GetServiceEx<IAuthenticationProvider>();
                if (service == null)
                {
                    //throw new Exception($"Authentication Service is not registered.");
                    var res = await AppHost.Rpc.Authenticate(new GN.Library.Shared.Authorization.AuthenticateCommand
                    {
                        UserName = userName,
                        Password = password

                    });
                    return res?.UserName != null;
                }
                return await this.serviceProvider.GetServiceEx<IAuthenticationProvider>().Authenticate(userName, password);
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to authenticate user. Err:{err.GetBaseException().Message}");
            }

            //return result; //true = user authenticated!

            return false;

        }
        private async Task<UserIdentityEntity[]> LoadIdentities()
        {
            try
            {
                var result = new List<UserIdentityEntity>();
                if (this.cache.TryGetValue("identities_cached_data", out var items) && items is UserIdentityEntity[] r)
                {
                    return r;
                }
                var skip = 0;
                var take = 100;
                while (true)
                {

                    var reply = await this.bus
                        .CreateMessage(new LoadIdentitiesCommand { Skip = skip, Take = take })
                        .CreateRequest()
                        .WaitFor(x => true)
                        .TimeOutAfter(15000, throwIfTimeOut: true);
                    var message = reply.Cast<LoadIdentitiesRpply>()?.Message?.Body;
                    if (message == null || message.Identities == null)
                    {
                        break;
                    }
                    result.AddRange(message.Identities);
                    if (message.Identities.Length < take)
                    {
                        break;
                    }
                    skip += take;
                }
                this.cache.Set("identities_cached_data", result.ToArray(), TimeSpan.FromMinutes(15));
                return result.ToArray();


            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to load identities. Err:{err.GetBaseException().Message}");
            }
            return new UserIdentityEntity[] { };
        }
        private async Task<UserEntity[]> GetUsersLocaly()
        {
            var result = new List<UserEntity>();
            try
            {
                var skip = 0;
                var take = 500;
                while (true)
                {
                    var _users = await
                        this.serviceProvider
                          .GetServiceEx<IUserLoader>()
                          .LoadUsers(skip, take);
                    if (_users.Length == 0)
                        break;
                    result.AddRange(_users);
                    skip += take;
                }
            }
            catch (Exception err)
            {
                throw;
            }
            return result.ToArray();

        }
        private async Task<UserEntity[]> GetUsersRemotely()
        {
            try
            {
                var result = new List<UserEntity>();
                var skip = 0;
                var take = 50;
                while (true)
                {
                    var reply = await this.bus.CreateMessage(new LoadUsersCommand() { Skip = skip, Take = take })
                        .UseTopic(Subjects.IdentityServices.LoadUsers)
                        .Options(cfg => { cfg.LocalOnly = true; })
                        .CreateRequest()
                        .WaitFor(x => true)
                        .TimeOutAfter(30000);
                    skip += take;
                    if (reply != null)
                    {
                        var batch = reply.Cast<LoadUsersReply>().Message?.Body?.Users ?? new UserEntity[] { };
                        if (batch.Length == 0)
                        {
                            break;
                        }
                        result.AddRange(batch);
                    }
                    else
                    {
                        this.logger.LogError(
                            $"Failed to load Users. This normally happens when User Services is not available.");
                        break;
                    }
                }
                this.logger.LogInformation(
                    $"{result.Count} User Successfully loaded.");
                return result.ToArray();
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to load user cache. Err:{err.GetBaseException().Message}");
            }
            return new UserEntity[] { };
        }

        private async Task<UserEntity[]> GetUsers(bool refersh = false)
        {
            if (this.users  == null || refersh)
            {
                try
                {
                    var localy = this.serviceProvider.GetServiceEx<IUserLoader>() != null;
                    this.users = localy
                        ? await this.GetUsersLocaly()
                        : await this.GetUsersRemotely();

                    this.logger.LogInformation(
                        $"UserCache successfully cached '{this.users.Length}' Users.");
                }
                catch (Exception err)
                {

                    this.logger.LogError(
                        $"An error occured while trying to load user cache. Err:{err.GetBaseException().Message}");
                }
                this.users = this.users ?? new UserEntity[] { };
                
            }
            return this.users;
        }
        private async Task<UserEntity[]> _GetUsers()
        {
            try
            {
                if (this.cache.TryGetValue("users_cached_data", out var items) && items is UserEntity[] r)
                {
                    return r;
                }
                var localy = this.serviceProvider.GetServiceEx<IUserLoader>() != null;
                var result = localy 
                    ? await this.GetUsersLocaly()
                    : await this.GetUsersRemotely();

                this.logger.LogInformation(
                    $"UserCache successfully cached '{result.Length}' Users.");
                this.cache.Set("users_cached_data", result, TimeSpan.FromMinutes(15));
                return result;
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to load user cache. Err:{err.GetBaseException().Message}");
            }
            return new UserEntity[] { };
        }

       

       

        public async Task<IQueryable<UserEntity>> GetQueryable()
        {
            return (await this.GetUsersRemotely()).AsQueryable();
        }

        public async Task<IQueryable<UserIdentityEntity>> GetIdentities()
        {
            var result = await this.LoadIdentities();
            return result.AsQueryable();
        }

        private async Task<UserIdentityEntity> GetIdentity(UserEntity entity)
        {
            var normal = ActiveDirectoryHelper.NormalizeUserName(entity.DomainName);
            UserIdentityEntity result = null;
            try
            {
                if (1 == 0)
                {
                    var ids = (await this.LoadIdentities());
                    result = ids.FirstOrDefault(x => string.Compare(x.UserPrincipalName, $"{normal.UserName}@{normal.DomianName}.local", true) == 0)
                        ?? ids.FirstOrDefault(x => string.Compare(x.UserPrincipalName, $"{normal.UserName}@{normal.DomianName}", true) == 0);
                }
                else
                {
                    result = (await this.bus.Rpc.Call<LoadIdentityCommand, LoadIdentityRpply>(new LoadIdentityCommand { UserName = entity.DomainName }))?.Identity;

                }
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to GetIdentity. {err.Message}");
            }
            return result;

        }
        public async Task<UserEntity> AuthenticateUser(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || !await this.Authenticate(userName, password))
            {
                return null;
            }
            var users = await this.GetUsers();
            var normal = ActiveDirectoryHelper.NormalizeUserName(userName);
            var user = users.FirstOrDefault(u => string.Compare(u.DomainName, $"{normal.DomianName}\\{normal.UserName}", true) == 0);
            if (user != null)
            {
                user.UserId = LibraryConventions.Instance.LoginNameToUserId(user.DomainName);
                if (user.Identity == null || user.Identity.Attributes.Count == 0)
                {
                    var identity = (await this.GetIdentity(user));// ?? new UserIdentityEntity { };
                    if (identity != null && !string.IsNullOrWhiteSpace(identity.UserPrincipalName))
                    {
                        identity.Token = this.tokenService.GenerateToken(identity.GetClaimsIdentity());
                        user.Identity = identity;
                        user.UserId = identity.UserPrincipalName;
                    }
                }
                else
                {
                    //user.Identity = new UserIdentityEntity();
                }
            }
            else
            {
                /// We failed to get an appropriate user from the 
                /// Users database (usually MS CRM)
                /// But since the user has been authenticated
                /// maybe we can generate a pseduo user
                /// 
                var identity = await this.GetIdentity(new UserEntity { DomainName = $"{normal.DomianName}\\{normal.UserName}", });
                if (identity != null && !string.IsNullOrWhiteSpace(identity.UserPrincipalName))
                {
                    identity.Token = this.tokenService.GenerateToken(identity.GetClaimsIdentity());
                    user = new UserEntity
                    {
                        DomainName = $"{normal.DomianName}\\{normal.UserName}",
                        Identity = identity,
                        UserId = identity.UserPrincipalName,
                    };
                    user.Identity = identity;
                    user.UserId = identity.UserPrincipalName;
                    user.FullName = identity.DisplayName;
                }
            }
            return user;
        }

        public async Task<UserEntity> GetUserByToken(string token)
        {
            var f = this.tokenService.ValidateToken(token);
            if (f != null)
            {
                var normalUser = ActiveDirectoryHelper.NormalizeUserName(f.Identity.Name);
                var pre_windows_user_name = $"{normalUser.DomianName?.Replace(".local", "")}\\{normalUser.UserName}";
                var users = await this.GetUsersRemotely();
                var result = users.FirstOrDefault(x => string.Compare(x.DomainName, pre_windows_user_name, true) == 0);

                if (result != null)
                {
                    result.UserId = f.Identity.Name;
                    return result;
                }

                if (result == null && f != null)
                {
                    /// We failed to get an appropriate user from the 
                    /// Users database (usually MS CRM)
                    /// But since the user has been authenticated
                    /// maybe we can generate a pseduo user
                    /// 
                    var identity = await this.GetIdentity(new UserEntity { DomainName = $"{normalUser.DomianName}\\{normalUser.UserName}", });
                    if (identity != null && !string.IsNullOrWhiteSpace(identity.UserPrincipalName))
                    {
                        identity.Token = this.tokenService.GenerateToken(identity.GetClaimsIdentity());
                        var user = new UserEntity
                        {
                            DomainName = $"{normalUser.DomianName}\\{normalUser.UserName}",
                            Identity = identity,
                            UserId = identity.UserPrincipalName,
                        };
                        user.Identity = identity;
                        user.UserId = identity.UserPrincipalName;
                        user.FullName = identity.DisplayName;
                        return user;
                    }

                }


            }
            return null;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000);
            while (!stoppingToken.IsCancellationRequested)
            {
                await this.GetUsers(true);
                await Task.Delay(15 * 60 * 1000);
            }
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
        public override  Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation(
                $"Starting UserIdentity Services");
            return base.StartAsync(cancellationToken);

            
        }
    }
}
