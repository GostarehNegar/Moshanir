using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GN.Library.SharePoint.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace GN.Library.SharePoint
{
    public interface ISPWebFactory
    {
        ISPWebAdapter GetWeb(SPConnectionString connectionString);
        ISPWebAdapter GetWeb(string connectionString);

        T GetWeb<T>(SPConnectionString connectionString) where T : ISPWebAdapter;
        T GetWeb<T>(string connectionString) where T : ISPWebAdapter;
    }
    class SPWeFactory : ISPWebFactory
    {
        private readonly IServiceProvider serviceProvider;

        public SPWeFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<T> GetWeb<T>() where T:ISPWebAdapter,new()
        {
            throw new NotImplementedException();
        }

        public ISPWebAdapter GetWeb(SPConnectionString connectionString)
        {
            var result = ActivatorUtilities.CreateInstance<Internals.SPWebAdapter>(this.serviceProvider);
            ClientContextExEx.DefaultServiceProvider = this.serviceProvider;
            var context = new ClientContextExEx(connectionString.Url, this.serviceProvider)
            {
                Credentials = connectionString.GetCredentials(),
                //ServiceProvider = this.serviceProvider,
            };

            result.Initialize(context);
            return result;
        }

        public T GetWeb<T>(SPConnectionString connectionString) where T : ISPWebAdapter
        {
            var result = ActivatorUtilities.CreateInstance<T>(this.serviceProvider);
            var context = new ClientContextExEx(connectionString.Url, this.serviceProvider)
            {
                Credentials = connectionString.GetCredentials(),
                //ServiceProvider = this.serviceProvider
            };

            result.Initialize(context);
            return result;
        }

        public ISPWebAdapter GetWeb(string connectionString)
        {
            return this.GetWeb(SPConnectionString.Parse(connectionString));
        }

        public T GetWeb<T>(string connectionString) where T : ISPWebAdapter
        {
            return this.GetWeb<T>(SPConnectionString.Parse(connectionString));
        }
    }
}
