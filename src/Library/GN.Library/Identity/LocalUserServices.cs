using GN;
using GN.Library.Helpers;
using GN.Library.Messaging;
using GN.Library.Shared.Entities;
using GN.Library.Shared.Identity;
using GN.Library.Shared.Internals;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GN.Library.Identity
{
    class LocalUserServices : BackgroundService, IUserServices
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<LocalUserServices> logger;
        private readonly ILocalUserRepository localRepository;
        private readonly ITokenService tokenService;
        private readonly IMemoryCache chache;

        public LocalUserServices(IServiceProvider serviceProvider, ILogger<LocalUserServices> logger, ILocalUserRepository local, ITokenService tokenService)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.localRepository = local;
            this.tokenService = tokenService;
            this.chache = this.serviceProvider.GetServiceEx<IMemoryCache>();
        }
        public async Task<bool> Authenticate(string userName, string password)
        {
            try
            {
                var service = serviceProvider.GetServiceEx<IAuthenticationProvider>();
                if (service == null)
                {
                    //throw new Exception($"Authentication Service is not registered.");
                    var res = await AppHost.Rpc.Authenticate(new Shared.Authorization.AuthenticateCommand
                    {
                        UserName = userName,
                        Password = password

                    });
                    return res?.UserName != null;
                }
                return await serviceProvider.GetServiceEx<IAuthenticationProvider>().Authenticate(userName, password);
            }
            catch (Exception err)
            {
                logger.LogError(
                    $"An error occured while trying to authenticate user. Err:{err.GetBaseException().Message}");
            }

            //return result; //true = user authenticated!

            return false;
        }

        private async Task<UserIdentityEntity> GetIdentity(UserEntity entity)
        {
            var normal = ActiveDirectoryHelper.NormalizeUserName(entity.DomainName);
            UserIdentityEntity result = null;
            try
            {
                if (1 == 0)
                {
                    var ids = await localRepository.GetAllIdentities(default);
                    result = ids.FirstOrDefault(x => string.Compare(x.UserPrincipalName, $"{normal.UserName}@{normal.DomianName}.local", true) == 0)
                        ?? ids.FirstOrDefault(x => string.Compare(x.UserPrincipalName, $"{normal.UserName}@{normal.DomianName}", true) == 0);
                }
                else
                {
                    var bus = this.serviceProvider.GetService<IMessageBus>();
                    result = (await bus.Rpc.Call<LoadIdentityCommand, LoadIdentityRpply>(new LoadIdentityCommand { UserName = entity.DomainName }))?.Identity;

                }
            }
            catch (Exception err)
            {
                logger.LogError(
                    $"An error occured while trying to GetIdentity. {err.Message}");
            }
            return result;

        }
        public async Task<UserEntity> AuthenticateUser(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || !await Authenticate(userName, password))
            {
                return null;
            }
            var users = await this.localRepository.GetAllUsers(default);
            var normal = ActiveDirectoryHelper.NormalizeUserName(userName);
            var user = users.FirstOrDefault(u => string.Compare(u.DomainName, $"{normal.DomianName}\\{normal.UserName}", true) == 0);
            if (user != null)
            {
                user.UserName = LibraryConventions.Instance.LoginNameToUserId(user.DomainName);
                if (user.Identity == null || user.Identity.Attributes.Count == 0)
                {
                    var identity = await GetIdentity(user);// ?? new UserIdentityEntity { };
                    if (identity != null && !string.IsNullOrWhiteSpace(identity.UserPrincipalName))
                    {
                        identity.Token = tokenService.GenerateToken(identity.GetClaimsIdentity());
                        user.Identity = identity;
                        user.UserName = identity.UserPrincipalName;
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
                var identity = await GetIdentity(new UserEntity { DomainName = $"{normal.DomianName}\\{normal.UserName}", });
                if (identity != null && !string.IsNullOrWhiteSpace(identity.UserPrincipalName))
                {
                    identity.Token = tokenService.GenerateToken(identity.GetClaimsIdentity());
                    user = new UserEntity
                    {
                        DomainName = $"{normal.DomianName}\\{normal.UserName}",
                        Identity = identity,
                        UserName = identity.UserPrincipalName,
                    };
                    user.Identity = identity;
                    user.UserName = identity.UserPrincipalName;
                    user.FullName = identity.DisplayName;
                }
            }
            return user;
        }

        public async Task<UserEntity> GetByUserName(string userId)
        {
            var result = await this.localRepository.GetUserByUserId(userId);
            if (result != null)
            {
                result.Identity = await this.GetIdentity(result);
            }
            return result;
        }

        public Task<IQueryable<UserIdentityEntity>> GetIdentities()
        {
            throw new NotImplementedException();
        }

        public async Task<IQueryable<UserEntity>> GetQueryable()
        {
            if (this.chache != null && this.chache.Get<UserEntity[]>("$users") != null)
            {
                return this.chache.Get<UserEntity[]>("$users").AsQueryable();
            }
            var users = await this.localRepository.GetAllUsers(default);
            if (this.chache != null)
            {
                this.chache.Set("$users", users.ToArray(), TimeSpan.FromMinutes(2));
            }
            return users.ToArray().AsQueryable();
        }

        public async Task<UserEntity> GetUserByToken(string token)
        {
            var f = tokenService.ValidateToken(token);
            if (f != null)
            {
                var normalUser = ActiveDirectoryHelper.NormalizeUserName(f.Identity.Name);
                var pre_windows_user_name = $"{normalUser.DomianName?.Replace(".local", "")}\\{normalUser.UserName}";
                var result = await localRepository.GetUserByUserId(f.Identity.Name);
                //var result = users.FirstOrDefault(x => string.Compare(x.DomainName, pre_windows_user_name, true) == 0);
                if (result != null)
                {
                    result.UserName = f.Identity.Name;
                    return result;
                }

                if (result == null && f != null)
                {
                    /// We failed to get an appropriate user from the 
                    /// Users database (usually MS CRM)
                    /// But since the user has been authenticated
                    /// maybe we can generate a pseduo user
                    /// 
                    var identity = await GetIdentity(new UserEntity { DomainName = $"{normalUser.DomianName}\\{normalUser.UserName}", });
                    if (identity != null && !string.IsNullOrWhiteSpace(identity.UserPrincipalName))
                    {
                        identity.Token = tokenService.GenerateToken(identity.GetClaimsIdentity());
                        var user = new UserEntity
                        {
                            DomainName = $"{normalUser.DomianName}\\{normalUser.UserName}",
                            Identity = identity,
                            UserName = identity.UserPrincipalName,
                        };
                        user.Identity = identity;
                        user.UserName = identity.UserPrincipalName;
                        user.FullName = identity.DisplayName;
                        return user;
                    }

                }


            }
            return null;

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
                    var bus = this.serviceProvider.GetServiceEx<IMessageBus>();
                    var reply = await bus.CreateMessage(new LoadUsersCommand() { Skip = skip, Take = take })
                        .UseTopic(LibraryConstants.Subjects.IdentityServices.LoadUsers)
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
                        logger.LogError(
                            $"Failed to load Users. This normally happens when User Services is not available.");
                        break;
                    }
                }
                logger.LogInformation(
                    $"{result.Count} User Successfully loaded.");
                return result.ToArray();
            }
            catch (Exception err)
            {
                logger.LogError(
                    $"An error occured while trying to load user cache. Err:{err.GetBaseException().Message}");
            }
            return new UserEntity[] { };
        }
        private async Task<UserEntity[]> GetUsersLocaly()
        {
            var result = new List<UserEntity>();
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {

                    var skip = 0;
                    var take = 500;
                    while (true)
                    {
                        var _users = await
                            scope.ServiceProvider
                              .GetServiceEx<IUserPrimitiveRepository>()
                              .LoadUsers(skip, take);
                        if (_users.Length == 0)
                            break;
                        result.AddRange(_users);
                        skip += take;
                    }
                }
            }
            catch (Exception err)
            {
                throw;
            }
            return result.ToArray();

        }
        private async Task<UserIdentityEntity[]> LoadIdentities(CancellationToken cancellationToken)
        {
            try
            {
                var result = new List<UserIdentityEntity>();

                var skip = 0;
                var take = 100;
                var bus = this.serviceProvider.GetServiceEx<IMessageBus>();
                while (true)
                {

                    var reply = await bus
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
                await this.localRepository.Upsert(result.ToArray(), cancellationToken);
                //cache.Set("identities_cached_data", result.ToArray(), TimeSpan.FromMinutes(15));
                return result.ToArray();


            }
            catch (Exception err)
            {
                logger.LogError(
                    $"An error occured while trying to load identities. Err:{err.GetBaseException().Message}");
            }
            return new UserIdentityEntity[] { };
        }

        private async Task<UserEntity[]> LoadUsers(CancellationToken cancellation)
        {
            var result = new UserEntity[] { };
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var localy = scope.ServiceProvider.GetServiceEx<IUserPrimitiveRepository>() != null;
                    result = localy
                        ? await GetUsersLocaly()
                        : await GetUsersRemotely();
                    await this.localRepository.Upsert(result, cancellation);

                }
                //logger.LogInformation(
                //    $"UserCache successfully cached '{result.Length}' Users.");
            }
            catch (Exception err)
            {

                logger.LogError(
                    $"An error occured while trying to load user cache. Err:{err.GetBaseException().Message}");
            }
            result = result ?? new UserEntity[] { };
            return result;

        }

        private async Task HandleQueryUser(IMessageContext<QueryUserRequest> context)
        {
            try
            {
                var message = context.Message.Body;
                UserEntity result = null;
                bool NoResponse = false;
                if (!string.IsNullOrWhiteSpace(message.Token))
                {
                    result = await this.GetUserByToken(message.Token);
                }
                else if (!string.IsNullOrWhiteSpace(message.UserName))
                {
                    result = await this.GetByUserName(message.UserName);
                }
                else if (!string.IsNullOrWhiteSpace(message.UserId))
                {
                    result = await this.GetById(message.UserId);
                }
                else if (!string.IsNullOrWhiteSpace(message.AttributeName))
                {
                    result = (await this.localRepository.GetAllUsers(context.CancellationToken))
                        .FirstOrDefault(x => x.GetAttributeValue<string>(message.AttributeName) == message.AttributeValue);
                }
                else
                {
                    NoResponse = true;
                }
                if (!NoResponse)
                {
                    await context.Reply(new QueryUserResponse { User = result });
                }
            }
            catch (Exception err)
            {
                await context.Reply(err);
            }

        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
            var bus = this.serviceProvider.GetServiceEx<IMessageBus>();
            await bus.CreateSubscription()
                .UseTopic(typeof(AuthenticateUserRequest))
                .AddSubject(LibraryConstants.Subjects.IdentityServices.AuthenticateUser)
                .UseHandler(async ctx =>
                {
                    var message = ctx.Message.Cast<AuthenticateUserRequest>()?.Body;
                    try
                    {
                        var res = await this.AuthenticateUser(message.UserName, message.Password);
                        await ctx.Reply(new AuthenticateUserResponse
                        {
                            User = res
                        });
                    }
                    catch (Exception err)
                    {
                        await ctx.Reply(err);
                    }
                })
                .Subscribe();
            await bus.CreateSubscription<QueryUserRequest>(HandleQueryUser)
                .AddSubject(LibraryConstants.Subjects.IdentityServices.QueryUser)
                .Subscribe();

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await this.LoadIdentities(stoppingToken);
                    }
                    catch (Exception err)
                    {

                    }
                    try
                    {
                        await this.LoadUsers(stoppingToken);
                    }
                    catch (Exception err)
                    {

                    }
                    this.logger.LogInformation(
                        $"Local User DB Updated. Users: { (await localRepository.GetAllUsers(stoppingToken)).Count()}, Identities:{(await localRepository.GetAllIdentities(stoppingToken)).Count()}");

                    await Task.Delay(15 * 60 * 1000, stoppingToken);
                }
            });
        }

        //public async Task<UserEntity> GetByUserName(string userId)
        //{
        //    return await this.localRepository.GetUserByUserId(userId);


        //}

        public async Task<UserEntity> GetById(string id)
        {
            var result = await this.localRepository.GetUserById(id);
            if (result != null)
            {
                result.Identity = await this.GetIdentity(result);
            }
            return result;
        }

        public async Task<UserEntity> QueryUserByAttributeValue(string attributeName, string attributeValue)
        {
            var q = await this.GetQueryable();
            return q.FirstOrDefault(x => x.GetAttributeValue(attributeName) == attributeValue);

        }
    }
}

