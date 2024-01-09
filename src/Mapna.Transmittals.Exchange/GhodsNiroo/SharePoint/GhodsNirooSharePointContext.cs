using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GN.Library.SharePoint;
using GN.Library.SharePoint.Internals;

namespace Mapna.Transmittals.Exchange.GhodsNiroo.SharePoint
{
    class GhodsNirooSharePointContext : IDisposable
    {
        private readonly ClientContext clientContext;
        private Web web;

        public GhodsNirooSharePointContext(ClientContext clientContext)
        {
            this.clientContext = clientContext;
        }

        public void Dispose()
        {
            this.clientContext.Dispose();
        }

        public Web GetWeb()
        {
            if (web == null)
            {
                web = this.clientContext.Web;
                this.clientContext.Load(web.Lists, w => w.Include(l => l.DefaultViewUrl, x => x.Title));
                this.clientContext.ExecuteQuery();
            }
            return web;
        }
        public Task EnsureLists(IServiceProvider serviceProvider)
        {
            this.GetWeb();
            return TransmittalsWebHelper.EnsureLists(this.clientContext, serviceProvider);
            


        }
        public async Task<ListEx<SPTransmittalItem>> GetTransmittalsList()
        {
            return (await GetWeb().GetListByPath("/Transmittal/")).Extend<ListEx<SPTransmittalItem>>();
        }
        public async Task<ListEx<SPJobItem>> GetJobs()
        {
            return (await GetWeb().GetListByPath("/Jobs/")).Extend<ListEx<SPJobItem>>();
        }
        public async Task<ListEx<SPDocLibItem>> GetDocs()
        {
            return (await GetWeb().GetListByPath("/Received Documents/")).Extend<ListEx<SPDocLibItem>>();
        }
        public async Task<SPJobItem> FindJobByInternalId(string internalId)
        {
            var jobs = (await this.GetJobs());//.FindJob(internalId);
            return await jobs.GetQueryable()
                .Where(x => x.InternalId == internalId)
                .FirstOrDefaultAsync();
        }
        public async Task<SPJobItem> CreateJob(SPJobItem item)
        {
            var jobs = (await this.GetJobs());
            await jobs.InsertItem(item);

            return item;
        }
        public async Task<SPJobItem> UpdateJob(SPJobItem item)
        {
            return await (await this.GetJobs())
                .UpdateItem(item);
        }

        public async Task<SPTransmittalItem> UploadTransmittal(string fileName, byte[] content, string transmittalNo, string projectCode, Action<SPTransmittalItem> configure = null)
        {
            var item = new SPTransmittalItem() { Title = fileName };
            var list = await this.GetTransmittalsList();
            item.SetFile(cfg => cfg.WithFileName(fileName).WithContent(content).WithOverride(true));
            item.TransmittalNo = transmittalNo;
            item.ProjectCode = projectCode;
            configure?.Invoke(item);
            var item_inserted = (await list.InsertItems(item)).FirstOrDefault();
            var item_read = await list.GetItemById<SPTransmittalItem>(item_inserted.Id);
            return item_read;
        }

        public async Task Test()
        {
            var lst = await GetTransmittalsList();
            var items = await lst.GetQueryable().Take(10).ToArrayAsync();
        }
        public async Task SendLogAsync(LogLevel level, string Scope, string fmt, params object[] args)
        {
            var message = string.Format(fmt, args);
            try
            {
                message = string.Format(fmt, args);
                await (await this.GetWeb().GetListByPath("/Log/"))
                    .InsertItem<SPLogItem>(new SPLogItem { Message = message, Title = Scope }.SetLevel(level));
            }
            catch { }

        }
        public async Task<bool> SetJobStatus(string id, string status, string reason)
        {
            var resuult = false;
            var job = await this.FindJobByInternalId(id);
            if (job != null)
            {
                job.Status = status;
                job.StatusReason = reason;
                await this.UpdateJob(job);
                resuult = true;
            }
            return resuult;
        }
        public Task<bool> SetJobFailed(string id, string reason) => this.SetJobStatus(id, SPJobItem.Schema.Statuses.Failed, reason);
        public Task<bool> SetJobCompleted(string id, string reason) => this.SetJobStatus(id, SPJobItem.Schema.Statuses.Completed, reason);

        public async Task<SPDocLibItem> UploadDocument(string fileName, byte[] content, Action<SPDocLibItem> configure)
        {
            var item = new SPDocLibItem() { Title = fileName };
            var list = await this.GetDocs();
            item.SetFile(cfg => cfg.WithFileName(fileName).WithContent(content).WithOverride(true));
            configure?.Invoke(item);
            var item_inserted = (await list.InsertItems(item)).FirstOrDefault();
            var item_read = await list.GetItemById<SPDocLibItem>(item_inserted.Id);
            return item_read;
        }
        public async Task<SPJobItem[]> GetPendingJobs()
        {
            var list = await this.GetJobs();
            var i = await list.GetQueryable()
                .Where(x => x.Status == SPJobItem.Schema.Statuses.InProgress)
                .ToArrayAsync();
            return i;

        }
    }
}
