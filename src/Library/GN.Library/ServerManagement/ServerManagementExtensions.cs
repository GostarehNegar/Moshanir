using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.ServerManagement
{
    public static class ServerManagementExtensions
    {
        public static IServiceCollection AddServerManagement(this IServiceCollection services, IConfiguration configuration, Action<ServerManagmentOptions> configure = null)
        {
            services.AddSingleton<ServerProcessControler>();
            services.AddHostedService(sp => sp.GetService<ServerProcessControler>());
            return services;
        }
    }
}
