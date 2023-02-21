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

namespace GN.Library.Identity
{



    class IdentityServices : IIdentityServices
    {
        private readonly IMessageBus bus;
        private readonly ILogger<IdentityServices> logger;
        private readonly IMemoryCache cache;
        private readonly ITokenService tokenService;
        private readonly IServiceProvider serviceProvider;

        public IdentityServices(IMessageBus bus,
                                ILogger<IdentityServices> logger,
                                IMemoryCache cache,
                                ITokenService tokenService,
                                IServiceProvider serviceProvider)
        {
            this.bus = bus;
            this.logger = logger;
            this.cache = cache;
            this.tokenService = tokenService;
            this.serviceProvider = serviceProvider;
        }
        

        
        public async Task<bool> Authenticate(string userName, string password)
        {
            bool result = false;
            await Task.CompletedTask;
            try
            {
                return ActiveDirectoryHelper.AuthenticateUser(userName, password); ;
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to Auhtniticate use using ActiveDirectory. Err:{err.GetBaseException().Message}");
            }
            return false;

        }
        private async Task<UserEntity[]> GetUsers()
        {
            try
            {
                if (this.cache.TryGetValue("users_cached_data", out var items) && items is UserEntity[] r)
                {
                    return r;
                }
                var reply = await this.bus.CreateMessage(new LoadUsersCommand())
                    .UseTopic(Subjects.IdentityServices.LoadUsers)
                    .CreateRequest()
                    .WaitFor(x => true)
                    .TimeOutAfter(5000);
                var result = reply.Cast<LoadUsersReply>().Message?.Body?.Users ?? new UserEntity[] { };
                this.cache.Set("users_cached_data", result, TimeSpan.FromMinutes(5));
                return result;
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to load user cache. Err:{err.GetBaseException().Message}");
            }
            return new UserEntity[] { };

        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        public async Task<IQueryable<UserEntity>> GetQueryable()
        {
            return (await this.GetUsers()).AsQueryable();
        }

        public Task<IQueryable<UserIdentityEntity>> GetIdentities()
        {
            throw new NotImplementedException();
        }
    }
}
