using Microsoft.SharePoint.Client;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace GN.Library.SharePoint.Internals
{
    
    public class ClientContextExEx : ClientContext
    {
        internal static IServiceProvider DefaultServiceProvider;
        public IServiceProvider ServiceProvider { get => (this.Tag as IServiceProvider) ?? DefaultServiceProvider; }
        public EventHandler OnDisposed;
        public ClientContextExEx(string url, IServiceProvider serviceProvider) : this(SPConnectionString.Parse(url), serviceProvider)
        {

        }
        public ClientContextExEx(SPConnectionString connectionString, IServiceProvider serviceProvider) : base(connectionString.Url)
        {
            this.Credentials = connectionString.GetCredentials();
            this.Tag = serviceProvider;
            DefaultServiceProvider = serviceProvider;
        }
        protected override void Dispose(bool disposing)
        {
            this.OnDisposed?.Invoke(this, null);
            base.Dispose(disposing);
        }
        public With<Web> WithWeb(params Expression<Func<Web, object>>[] selector) => new With<Web>(this.Web, selector);

        public async Task HHH()
        {
            var w = new MyWeb(this, this.Web.Path);

        }
    }
    class MyWeb : Web
    {
        public MyWeb(ClientRuntimeContext ctx, ObjectPath path) : base(ctx, path)
        {

        }
    }
}
