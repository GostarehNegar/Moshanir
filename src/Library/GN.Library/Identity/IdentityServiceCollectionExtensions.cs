using GN.Library.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GN.Library.Identity.ActiveDirectory;
using GN.Library.Authorization;
using GN.Library.Messaging;
using GN.Library.Shared.Internals;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServiceCollectionExtensions
    {
        //public static IServiceCollection AddIdentityServicesEx(this IServiceCollection services, IConfiguration configuration, Action<IdentityServiceOptions> configure = null)
        //{
        //    var options = new IdentityServiceOptions();
        //    services.AddSingleton(options);
        //    services.AddSingleton<IdentityServices>();
        //    services.AddSingleton<IIdentityServices>(sp=> sp.GetService<IdentityServices>());



        //    return services;
        //}
        public static IServiceCollection AddActiveDirectoryIdentityServices(this IServiceCollection services, IConfiguration configuration, Action<ActiveDirectoryOptions> configure)
        {
            //var options = new ActiveDirectoryOptions();
            var options = configuration?
                .GetSection("identity")?
                .Get<ActiveDirectoryOptions>() ?? new ActiveDirectoryOptions();
            configure?.Invoke(options);
            if (!options.Disabled)
            {
                services.AddSingleton(options);
                services.AddSingleton<ActiveDirectoryProviderEx>();
                services.AddTransient<IAuthenticationProvider>(sp => sp.GetService<ActiveDirectoryProviderEx>());
                services.AddTransient<IAuthorizationService>(sp => sp.GetService<ActiveDirectoryProviderEx>());
                services.AddTransient<IUserIdentityProvider>(sp => sp.GetService<ActiveDirectoryProviderEx>());
                services.AddTransient<IMessageHandler, AuthenticateCommandHandler>();
                services.AddTransient<IMessageHandlerConfigurator, ActiveDirectoryProviderEx>();
                services.AddMessagingServices(cfg =>
                {
                    cfg.Register(subs =>
                    {
                        var p = subs.ServiceProvider.GetService<ActiveDirectoryProviderEx>();
                        subs.UseTopic(typeof(LoadIdentityCommand))
                        .UseHandler(ctx =>
                        {
                            return subs.ServiceProvider.GetService<ActiveDirectoryProviderEx>().HandleLoadUserCommand(ctx);
                        });
                    });
                });
            }

            return services;
        }
    }
}
