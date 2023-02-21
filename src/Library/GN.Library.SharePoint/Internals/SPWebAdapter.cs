using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;

namespace GN.Library.SharePoint.Internals
{

    public class SPWebAdapter : ISPWebAdapter
    {
        protected ILogger logger;
        private Web _web;
        private List<SPListAdapter> _lists;

        public ClientContextExEx Context { get; protected set; }

        public Task<string> Title
        {
            get { return this.Context.Web.GetPropertyAsync(x => x.Title); }
        }


        protected async Task<Web> Load(bool refersh = false)
        {

            if (_web == null || refersh)
            {
                _web = this.Context.Web;
                this.Context.Load(this.Context.Web, w => w.Title, w => w.Lists,
                    w => w.Lists.Include(l => l.DefaultViewUrl,
                                         l => l.Title));
                await this.Context.ExecuteQueryAsync();
                this._lists = _web.Lists.Select(x => new SPListAdapter(x))
                    .ToList();
            }
            return this._web;
        }

        public SPWebAdapter()
        {

        }

        public void Initialize(ClientContextExEx context)
        {
            this.Context = context;
            this.logger = context.ServiceProvider.GetServiceEx<ILoggerFactory>().CreateLogger(this.GetType());

        }
        public void KKK<T1, T2>(Expression<Func<T1, T2>> selector)
        {

        }

        public void Test()
        {
            //var tag = this.Context.Web.GetProperty(x => x.Tag);
            var title = this.Context.Web.GetProperty(x => x.Title);

            //this.GetWeb();
        }

        public async Task<IEnumerable<ISPListAdapter>> GetLists(bool refersh = false)
        {
            await this.Load(refersh);
            return this._lists;
        }
        public async Task<ISPListAdapter> GetListByDefaultUrl(string path, bool refersh = false)
        {
            return (await this.GetLists(refersh))
                .FirstOrDefault(x => x.DefaultViewUrl.ToLowerInvariant().Contains(path?.ToLowerInvariant()));

        }

        public async Task<ISPListAdapter<T>> GetListByDefaultUrl<T>(string path, bool refersh = false) where T : SPItem
        {
            await this.Load(refersh);
            var ret = this._lists
                .FirstOrDefault(x => x.DefaultViewUrl.ToLowerInvariant().Contains(path?.ToLowerInvariant()));
            return ret == null ? null : new SPListAdapter<T>(ret.SPList);

            //result.Initialize(ret.l)

        }

        public async Task<T> GetListByDefaultUrl<T, TItem>(string path, bool refersh = false) where T : SPListAdapter<TItem> where TItem : SPItem
        {
            var l = (await GetListByDefaultUrl(path, refersh)) as SPListAdapter;
            var result = Activator.CreateInstance<T>();
            result.Initialize(l.SPList);
            return result;
        }

        public async Task<bool> EnsureConnection(bool ThrowIfFailed = true)
        {
            try
            {
                var web = await this.Load();
                return true;
            }
            catch (Exception)
            {
                if (ThrowIfFailed)
                {
                    throw;
                }
            }
            return false;
        }
    }
}
