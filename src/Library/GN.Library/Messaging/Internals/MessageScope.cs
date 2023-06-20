using Microsoft.Extensions.DependencyInjection;
using System;

namespace GN.Library.Messaging.Internals
{
    class MessageScope : IServiceScope
    {
        private IServiceScope scope;
        private readonly IServiceProvider serviceProvider;

        public IServiceProvider ServiceProvider => scope?.ServiceProvider ?? serviceProvider;
        public MessageScope(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public IServiceScope CreateScope()
        {
            this.scope?.Dispose();
            this.scope = this.serviceProvider.CreateScope();
            return this;
        }

        public void Dispose()
        {
            this.scope?.Dispose();
            this.scope = null;
        }
    }
}


