using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Functional
{
    public interface ILocator : IServiceProvider { }
    public class Locator : ILocator
    {
        public static Locator Current => new Locator();
        private static IServiceProvider serviceProvider;
        public static void Initialize(IServiceProvider provider)
        {
            serviceProvider = provider;
        }
        public object GetService(Type serviceType)
        {
            serviceProvider = serviceProvider ?? AppHost.Services?.Provider ?? new ServiceCollection().BuildServiceProvider();
            return serviceProvider.GetService(serviceType);
        }

      
    }
}
