using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;


namespace GN.Library.SharePoint
{
    public interface IClientContextFactory
    {
        ClientContext CreateContext(SPConnectionString connectionString);
        ClientContext CreateContext(string connectionString);
        IServiceProvider ServiceProvider { get; }
    }
    public class ClientContextFactory : IClientContextFactory
    {
        public IServiceProvider ServiceProvider { get; }

        public ClientContextFactory(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public class Builder
        {
            private readonly IServiceCollection services;
            private Action<IConfiguration, IServiceCollection> configure;
            public Builder(IServiceCollection services)
            {
                this.services = services;
            }
            public Builder ConfigureServices(Action<IConfiguration, IServiceCollection> configure)
            {
                this.configure = configure;
                return this;
            }
            public IClientContextFactory Build()
            {
                var host = new HostBuilder()
                    .UseDefaultServiceProvider(s => s.ValidateScopes = false)
                    .ConfigureLogging(l => l.AddConsole())
                    .ConfigureServices((c, s) => {
                        this.configure?.Invoke(c.Configuration, s);
                    }).Build();
                return new ClientContextFactory(host.Services);
            }

        }
        ClientContext IClientContextFactory.CreateContext(SPConnectionString connectionString)
        {
            return CreateContext(connectionString, this.ServiceProvider);
        }
        ClientContext IClientContextFactory.CreateContext(string connectionString)
        {
            return CreateContext(connectionString, this.ServiceProvider);
        }

        public static ClientContext CreateContext(SPConnectionString connectionString, IServiceProvider serviceProvider)
        {
            ClientContextExtensions.DefualtServiceProvider = serviceProvider == null ? ClientContextExtensions.DefualtServiceProvider : serviceProvider;
            return new ClientContext(connectionString.Url)
            {
                Credentials = connectionString.GetCredentials(),
                Tag = serviceProvider
            };
        }
        public static ClientContext CreateContext(string connectionString, IServiceProvider serviceProvider)
        {
            return CreateContext(SPConnectionString.Parse(connectionString), serviceProvider);
        }
        public static Builder GetBuilder(IServiceCollection services = null)
        {
            return new Builder(services ?? new ServiceCollection());
        }

    }

    public static class ClientContextExtensions
    {
        internal static IServiceProvider DefualtServiceProvider;
        public static IServiceProvider ServiceProvider(this ClientContext context)
        {
            return context.Tag as IServiceProvider ?? DefualtServiceProvider;
        }
    }
}
