using GN.Library.SharePoint;
using GN.Library.SharePoint.Internals;
using Mapna.Transmittals.Exchange.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.SharePoint.Client;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.IO;

namespace Mapna.Transmittals.Exchange.Internals
{
    public interface ITransmittalRepository : IDisposable
    {
        Task<ITransmittalData> Create(SPTransmittalItem transmittal);
        Task<SPMasterListItem> FindInMasterList(string documentNumber);
        Task<SPJobItem> FindJob(string sourcId);
        Task<SPJobItem> FindJobByInternalId(string internalId);
        Task<SPJobItem> CreateJob(SPJobItem job);
        Task<SPItem> UpdateJob(SPJobItem job);
        Task DeleteJob(SPJobItem job);
        Task SetJobStatus(string internalId, string status, string reason = null);

        Task<SPJobItem[]> GetInProgressJobs();
        Task<SPJobItem[]> GetPendingJobs();

        Task<int> CountTransmitalls();
        Task<SPTransmittalItem> GetOrAddTransmittal(string referenceNumber, Action<SPTransmittalItem> configure, string action = null);
        Task<SPTransmittalItem> GetTransmittal(string transmittalNumber);
        Task<SPTransmittalItem> GetTransmittalById(int id);
        void SendLog(LogLevel level, string fmt, params object[] args);
        Task<string> UploadDoc(string name, System.IO.Stream stream, Action<SPDocLibItem> configure = null);
        Task<string> UploadDoc(string relativePath, string name, System.IO.Stream stream, Action<SPDocLibItem> configure = null);

        Task<SPDocLibItem> GetDocumentByPath(string path);




        Task<SPDocLibItem[]> GetDocumentsByTransmittalId(int transmittalId);
        Task<SPDocLibItem[]> GetDocumentsByTransmittal(string transmittal);
        Task AttachTransmittalLetter(int id, string fileName, byte[] content);
        Task Test(string path);

        Task<SPTransmittalItem> UpdateTransmittal(SPTransmittalItem item);

        Task SetTransmittalIssueState(string transmittalNumber, SPTransmittalItem.Schema.IssueStates state);

        Task<SPTransmittalItem[]> GetWaitingTransmittals();

        Task<SPItem> GetCompany(string comcod);
        Task<SPItem> GetDiscipline(string title);

        string ToAbsoultePath(string serverRelativePath);
        Task<SPDocLibItem> UpdateDocument(SPDocLibItem item);
        Task DeleteDocument(SPDocLibItem item);
    }



    class TransmittalRepository : ITransmittalRepository
    {
        private readonly IClientContextFactory factory;
        private readonly TransmittalsExchangeOptions options;
        private ClientContext context;

        public void Dispose()
        {
            context?.Dispose();
        }
        public TransmittalRepository(IClientContextFactory factory, TransmittalsExchangeOptions options)
        {
            this.factory = factory;
            this.options = options;
        }
        public ClientContext GetContext()
        {
            return factory.CreateContext(SPConnectionString.Parse(options.ConnectionString));
            //if (context == null)
            //{
            //    context?.Dispose();
            //    context = factory.CreateContext(SPConnectionString.Parse("http://projects.gnco.ir/mapna"));
            //}
            //return context;
        }
        private SPTransmittalList list;
        private SPTransmittalWeb web;
        public SPTransmittalWeb GetWeb()
        {
            if (web == null)
            {
                web = GetContext().Web.Extend<SPTransmittalWeb>();
            }
            return web;
        }
        public async Task<SPTransmittalList> GetList()
        {
            return await GetWeb().GetTransmitalsList();
            if (list == null)
            {
                var ctx = GetContext();
                list = await ctx.Web.Extend<SPTransmittalWeb>().GetTransmitalsList();
            }
            return list;
        }
        public async Task<SPJobsList> GetJobs()
        {
            return await GetWeb().GetJobsList();

        }
        public async Task<ITransmittalData> Create(SPTransmittalItem transmittal)
        {
            using (var ctx = GetContext())
            {
                var list = await GetList();
                var result = await list.InsertItems(transmittal);
                return result.FirstOrDefault();
            }
            //var ctx = this.GetContext();
            //{
            //    ctx.Dispose();
            //    var list = await ctx.Web.Extend<SPTransmittalWeb>().JJJ();
            //    var result = await list.InsertItems(new SPTransmittalItem { });
            //    ctx.Load(ctx.Web);
            //    ctx.ExecuteQuery();
            //    return result.FirstOrDefault();
            //}
        }

        public async Task<SPMasterListItem> FindInMasterList(string documentNumber)
        {
            using (var ctx = GetContext())
            {
                var list = await ctx.Web.Extend<SPTransmittalWeb>().GetMasterList();
                var q = list.GetQueryable(cfg => { });
                return await q.Where(x => x.DocumentNumber == documentNumber).FirstOrDefaultAsync();
            }
        }

        public async Task<int> CountTransmitalls()
        {
            var list = await GetList();
            return (await list.GetQueryable<SPItem>().ToArrayAsync()).Length;
        }

        public async Task<SPJobItem> FindJob(string sourcId)
        {
            return await (await GetJobs()).FindJob(sourcId);
        }

        public async Task<SPJobItem> CreateJob(SPJobItem job)
        {
            return await (await GetJobs()).GetOrCreateItem(job);
        }

        public async Task<SPJobItem[]> GetInProgressJobs()
        {
            var jobs = await GetWeb().GetJobsList();
            return await jobs.GetQueryable()
                .Where(x => x.Status == SPJobItem.Schema.Statuses.InProgress)
                .ToArrayAsync();

        }

        public async Task<SPItem> UpdateJob(SPJobItem job)
        {
            var jobs = await GetWeb().GetJobsList();
            return await jobs.UpdateItem(job);
        }

        public async Task DeleteJob(SPJobItem job)
        {
            var jobs = await GetWeb().GetJobsList();
            await jobs.DeleteItem(job);
        }

        public async Task<SPJobItem> FindJobByInternalId(string internalId)
        {
            var jobs = await GetWeb().GetJobsList();
            return await jobs.GetQueryable()
                .Where(x => x.InternalId == internalId)
                .FirstOrDefaultAsync();
        }
        public void SendLog(LogLevel level, string fmt, params object[] args)
        {

            _ = SendLogAsync(level, fmt, args);

        }
        public async Task SendLogAsync(LogLevel level, string fmt, params object[] args)
        {
            try
            {
                var message = fmt;
                try
                {
                    message = string.Format(fmt, args);
                }
                catch { }
                //Monitor.Enter(this.web);
                try
                {
                    var web = await (await this.GetWeb().GetListByPath("/Log/"))
                        .InsertItem<SPLogItem>(new SPLogItem { Message = message, Title = "Log" }.SetLevel(level));
                }
                finally
                {
                    //Monitor.Exit(this.web);
                }
            }
            catch
            {

            }

        }

        public async Task SetJobStatus(string internalId, string status, string reason = null)
        {
            var job = await this.FindJobByInternalId(internalId);
            if (job == null)
            {
                throw new TransmitalException($"Job Not Found", false);
            }
            job.Status = status;
            job.StatusReason = reason;

            var list = await this.GetWeb().GetJobsList();
            await list.UpdateItem(job);
        }

        public async Task<string> UploadDoc(string name, System.IO.Stream stream, Action<SPDocLibItem> configure = null)
        {
            var list = await this.GetWeb().GetDocLib();
            var path = await list.With(x => x.RootFolder, x => x.RootFolder.ServerRelativeUrl)
                .DoAsync(x => x.RootFolder.ServerRelativeUrl);
            var res = await this.GetContext()
                .SaveBinaryDirectAsync($"{path}/{Path.GetFileName(name)}", stream, true);
            if (configure != null)
            {
                await res.With(x => x.ListItemAllFields).DoAsync(async x =>
                {
                    var _item = new SPDocLibItem();
                    //_item.Init(x.ListItemAllFields);
                    configure(_item);
                    list.Copy(_item, x.ListItemAllFields);
                    //_item.ListItem["Transmittal"] = 122;
                    x.ListItemAllFields.Update();
                    await list.Context.ExecuteQueryAsync();
                });
            }
            return await res.With(f => f.ServerRelativeUrl).DoAsync(x => x.ServerRelativeUrl);
        }
        public async Task<string> UploadDocEx(string name, System.IO.Stream stream)
        {
            var list = await this.GetWeb().GetDocLib();
            var p = await list.With(x => x.RootFolder, x => x.RootFolder.ServerRelativeUrl).DoAsync(x => x.RootFolder.ServerRelativeUrl);
            //var path = $"Mapna/Temp/1.tmp";
            var path = $"{p}/{Path.GetFileName(name)}";
            var res = await this.GetContext().SaveBinaryDirectAsync(path, stream, true);
            res.With(x => x.ListItemAllFields).Do(x =>
            {
                x.ListItemAllFields["Transmitall"] = 122;
                x.ListItemAllFields.Update();
                //res.Update();
                list.Context.ExecuteQuery();


            });

            return await res.With(f => f.ServerRelativeUrl).DoAsync(x => x.ServerRelativeUrl);
        }

        public async Task<SPDocLibItem[]> GetDocumentsByTransmittalId(int transmittalId)
        {
            var list = await this.GetWeb().GetDocLib();
            return await list.GetQueryable()
                .Where(x => x.TransmittalId == transmittalId)
                .ToArrayAsync();

        }

        public async Task<SPDocLibItem[]> GetDocumentsByTransmittal(string transmittal)
        {
            var list = await this.GetWeb().GetDocLib();
            return await list.GetQueryable()
                .Where(x => x.Transmittal == transmittal)
                .ToArrayAsync();

        }

        public async Task<SPTransmittalItem> GetOrAddTransmittal(string referenceNumber, Action<SPTransmittalItem> configure, string action = null)
        {
            var list = await this.GetWeb().GetTransmitalsList();
            var item = await list.GetQueryable()
                .Where(x => x.LetterNo == referenceNumber)
                .FirstOrDefaultAsync();
            ContentType ct = null;
            if (!string.IsNullOrWhiteSpace(action))
            {
                ct = list.GetContentTypeByName(action);
                if (ct == null)
                {
                    throw new Exception(
                        $"Invalid Action. Action ':{action}' is invalid because there is no corresponding content type for this action. ");
                }

            }
            if (item != null)
            {
                configure?.Invoke(item);
                if (ct != null)
                {
                    item.SetAttributeValue("ContentTypeId", ct.Id);
                }
                return await list.UpdateItem(item);
            }
            else
            {
                item = new SPTransmittalItem()
                {
                    LetterNo = referenceNumber
                };
                if (ct != null)
                {
                    item.SetAttributeValue("ContentTypeId", ct.Id);
                }
                configure?.Invoke(item);
                // 
                //
                try
                {
                    return await list.InsertItem(item);
                }
                catch (Exception err)
                {
                    if (item.Id == 0)
                    {
                        item = await list.GetQueryable()
                           .Where(x => x.LetterNo == referenceNumber)
                           .FirstOrDefaultAsync();
                    }
                }
                if (item == null || item.Id == 0)
                {
                    throw new TransmitalException("Failed to Insert Transmittal.");
                }
                return item;
            }

        }

        public async Task<SPJobItem[]> GetPendingJobs()
        {
            var list = await this.GetWeb().GetJobsList();
            var i = await list.GetQueryable()
                .Where(x => x.Status == SPJobItem.Schema.Statuses.InProgress)
                .ToArrayAsync();
            return i;

        }

        public async Task AttachTransmittalLetter(int id, string fileName, byte[] content)
        {
            var list = await this.GetWeb().GetTransmitalsList();
            await list.AttachByWebService(id, fileName, content);
        }

        public async Task Test(string path)
        {
            var docs = await this.GetWeb().GetDocLib();
            var lst = await this.GetWeb().GetTransmitalsList();
            var ttt = lst.GetContentTypes();
            return;
            var trans = await this.GetTransmittal("MD2-MOS-11");
            var atts = await SPListExtensions.GetAttachments(trans);
            var list = await this.GetWeb().GetTransmitalsList();
            var folder = await list.GetRootFolder();
            //await .ListItem.With(x => x.fol)
            //    .DoAsync(item => {
            //        var ggg = item.AttachmentFiles;
            //    });
            var f = this.GetWeb().GetFolderByServerRelativeUrl("/ardakan/Lists/TrList/Attachments/35202/");
            await f.With(x => x.Files)
                .DoAsync(async ff =>
                {

                    var l = ff.Files;


                });


            //var doc = this.GetWeb().GetFileByServerRelativeUrl(path);
            //await doc.With(x => x.ListItemAllFields)
            //    .DoAsync(x => {

            //        var f = x.ListItemAllFields;
            //    });


            //var folders = await docs.GetRootFolder();
            //var names = folders.Folders.Select(x => x.Name).ToArray();
            //var items = await docs.GetQueryable().ToArrayAsync();

        }

        public async Task<string> UploadDoc(string relativePath, string name, Stream stream, Action<SPDocLibItem> configure = null)
        {
            {
                //relativePath = string.IsNullOrWhiteSpace(relativePath) ? "/" : relativePath;
                var list = await this.GetWeb().GetDocLib();
                var path = await list.With(x => x.RootFolder, x => x.RootFolder.ServerRelativeUrl)
                    .DoAsync(x => x.RootFolder.ServerRelativeUrl);
                var res = await this.GetContext()
                    .SaveBinaryDirectAsync($"{path}{relativePath}{Path.GetFileName(name)}", stream, true);
                if (configure != null)
                {
                    await res.With(x => x.ListItemAllFields).DoAsync(async x =>
                    {
                        var _item = new SPDocLibItem();
                        //_item.Init(x.ListItemAllFields);
                        configure(_item);
                        list.Copy(_item, x.ListItemAllFields);
                        //_item.ListItem["Transmittal"] = 122;
                        x.ListItemAllFields.Update();
                        await list.Context.ExecuteQueryAsync();
                    });
                   // return $"{path}{relativePath}/{Path.GetFileName(name)}";
                }
                return await res.With(f => f.ServerRelativeUrl).DoAsync(x => x.ServerRelativeUrl);
            }
        }

        public async Task<SPTransmittalItem> GetTransmittal(string transmittalNumber)
        {
            return await (await this.GetWeb()
                .GetTransmitalsList())
                .GetQueryable()
                .Where(x => x.TransmittalNo == transmittalNumber)
                .FirstOrDefaultAsync();

        }

        public async Task<string> UploadDoc(string transmittalNo, string documentNumber, string name, Stream stream, Action<SPDocLibItem> configure = null)
        {
            var trans = await this.GetTransmittal(transmittalNo);
            if (trans == null)
            {
                throw new TransmitalException(
                    $"Transmittal Not Found: '{transmittalNo}'");
            }
            var document = await this.FindInMasterList(documentNumber);
            if (document == null)
            {
                throw new TransmitalException(
                    $"Document Not Found: {documentNumber}");
            }
            var result = await this.UploadDoc($"/{trans.TransmittalNo}", name, stream, cfg =>
            {
                cfg.TransmittalId = trans.Id;
                cfg.DocumentNumberId = document.Id;
                configure?.Invoke(cfg);
            });
            return result;


        }

        public async Task<SPDocLibItem> GetDocumentByPath(string path)
        {
            var doc = this.GetWeb().GetFileByServerRelativeUrl(path);
            int? id = null;
            await doc.With(x => x.ListItemAllFields)
                .DoAsync(x =>
                {
                    try
                    {
                        var f = x.ListItemAllFields;

                        id = (int)x.ListItemAllFields["ID"];
                    }
                    catch { }
                });
            if (!id.HasValue)
                return null;
            var item = await (await this.GetWeb().GetDocLib())
                .GetItemById<SPDocLibItem>(id.Value);

            return item;
        }

        public async Task<SPTransmittalItem> GetTransmittalById(int id)
        {
            return await (await this.GetWeb().GetTransmitalsList())
                .GetItemById<SPTransmittalItem>(id);
        }

        public async Task<SPTransmittalItem> UpdateTransmittal(SPTransmittalItem item)
        {
            return await (await this.GetWeb()
                 .GetTransmitalsList())
                 .UpdateItem(item);
        }

        public async Task SetTransmittalIssueState(string transmittalNumber, SPTransmittalItem.Schema.IssueStates state)
        {

            var trans = await this.GetTransmittal(transmittalNumber);
            if (trans == null)
            {
                throw new Exception("Transmittal Not Found");
            }
            trans.SendFormal = "Yes";
            trans.IssueState = state.ToString();
            await UpdateTransmittal(trans);
        }

        public async Task<SPTransmittalItem[]> GetWaitingTransmittals()
        {
            var list = (await this.GetWeb()
                 .GetTransmitalsList());
            return (await list.GetQueryable()
                .Where(x => x.IssueState == "Waiting" && x.From == "MOS")
                .ToArrayAsync())
                .Where(x => x.ToSI == "MD2")
                .ToArray();

        }

        public async Task<SPItem> GetCompany(string comcod)
        {
            var lst = await this.GetWeb().GetListByPath("company");
            var companies = await lst.GetQueryable<SPItem>().ToArrayAsync();
            var res = companies.FirstOrDefault(x => x.GetAttibuteValue<string>("ComCod") == comcod);
            return res;


        }

        public async Task<SPItem> GetDiscipline(string title)
        {
            var lst = await this.GetWeb().GetListByPath("Discipline");
            return await lst.GetQueryable<SPItem>()
                .Where(x => x.Title == title)
                .FirstOrDefaultAsync();

        }

        public string ToAbsoultePath(string serverRelativePath)
        {
            try
            {
                return Uri.TryCreate(this.GetContext().Url, UriKind.Absolute, out var uri)
                    ? uri.GetLeftPart(UriPartial.Authority) + serverRelativePath
                    : "";
            }
            catch
            {

            }
            return "";


        }

        public async Task<SPDocLibItem> UpdateDocument(SPDocLibItem item)
        {
            return await(await this.GetWeb()
                .GetDocLib())
                .UpdateItem(item);
            
        }

        public async Task DeleteDocument(SPDocLibItem item)
        {
            await(await this.GetWeb()
               .GetDocLib())
               .DeleteItem(item);
        }
    }

}