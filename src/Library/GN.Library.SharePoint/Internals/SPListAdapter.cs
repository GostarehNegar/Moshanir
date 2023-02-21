using GN.Library.SharePoint.Internals.LinqQuery;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Linq.Expressions;

namespace GN.Library.SharePoint.Internals
{
    public class SPListAdapter : ISPListAdapter
    {
        private List _list;
        private bool loaded;
        private SPListSchema _schema;

        public List SPList => this._list;
        public ClientContextExEx Context { get; private set; }

        public string DefaultViewUrl => this._list.DefaultViewUrl;

        public With<List> WithList(params Expression<Func<List, object>>[] selector) => new With<List>(this._list, selector);

        public SPListAdapter()
        {

        }
        public SPListAdapter(List list)
        {

            Initialize(list);
        }
        public ISPListAdapter Initialize(List list)
        {
            this._list = list;
            this.Context = list.Context.Extend_Deprecated();
            return this;
        }
        protected Field GetFieldByName(string name, bool ThrowIfNotFound = true)
        {
            var result = this.SPList.Fields.FirstOrDefault(x => x.InternalName == name);
            if (result == null && ThrowIfNotFound)
            {
                throw new Exception(
                       $"Field Not Found. '{name}'");
            }
            return result;
        }
        public async Task<bool> ValidateQuery(SPQueryOptions query)
        {
            await this.Load();
            if (query.Columns != null)
            {
                var non_exisiting = query.Columns.FirstOrDefault(x => GetFieldByName(x) == null);
            }
            return true;
        }

        public async Task Load(bool refersh = false)
        {
            if (!loaded || refersh)
            {
                this.Context.Load(this._list, l =>
                l.Fields.Include(f => f.Title, f => f.InternalName, f => f.FieldTypeKind, f => f.TypeAsString, f => f.ReadOnlyField));
                //this.Context.Load(this.List.Fields, f=>f.Include(f=>f.Title, f=>f.InternalName)
                await this.Context.ExecuteQueryAsync();
                this._schema = new SPListSchema(this._list.Fields, typeof(SPItem));
                this.loaded = true;
            }
        }
        public async Task<IEnumerable<SPItem>> GetItems()
        {
            await Load();
            var query = CamlQuery.CreateAllItemsQuery(10);
            ListItemCollection items = this._list.GetItems(query);
            this.Context.Load(items);
            await this.Context.ExecuteQueryAsync();
            return items.Select(x => new SPItem().Init(x, this))
                .ToArray();

        }
        public async Task<IEnumerable<SPItem>> GetItems(CamlQuery query)
        {
            await Load();
            ListItemCollection items = this._list.GetItems(query);
            this.Context.Load(items);
            await this.Context.ExecuteQueryAsync();
            return items.Select(x => new SPItem().Init(x, this))
                .ToArray();

        }
        public async Task Test()
        {
            await Load();
            var g = this._list.GetProperty(x => x.EventReceivers);
            this.SPList.EventReceivers.Add(new EventReceiverDefinitionCreationInformation
            {
                EventType = EventReceiverType.ItemAdded,
                ReceiverAssembly = this.GetType().AssemblyQualifiedName,
                ReceiverClass = "EVent",
                ReceiverName = "My",
                ReceiverUrl = "klkkk"
            });
            this.SPList.Update();
            this.Context.ExecuteQuery();
            //var fields = this._list.Fields;
            //ListItemCollection items = this._list.GetItems(query);
            //this.Context.Load(items);
            //await this.Context.ExecuteQueryAsync();


        }

    }
    public class SPListAdapter<T> : SPListAdapter, ISPListAdapter<T> where T : SPItem
    {
        public SPListAdapter() : base()
        {

        }
        public SPListAdapter(List list) : base(list)
        {

        }

        public async ValueTask<bool> IsDocumetLibrary()
        {
            await this.Load();
            return this.SPList.BaseType == BaseType.DocumentLibrary;
        }

        public async Task<T> AddItem(T item)
        {
            await this.Load();
            var _item = this.SPList.AddItem(new ListItemCreationInformation { });
            Copy(item, _item);
            _item.Update();
            await this.Context.ExecuteQueryAsync();
            var result = Activator.CreateInstance<T>();
            result.Init(_item, this);
            return result;
        }

        public async Task DeleteItem(T item)
        {
            await this.Load();
            var _item = item.ListItem;
            if (_item != null)
            {
                _item.DeleteObject();
            }
            await this.Context.ExecuteQueryAsync();
        }

        public IAsyncQueryable<T> GetQueryable(SPQueryOptions options)
        {

            return new LinqQuery.Queryable<T>(new QueryExecutor_Deprecated<T>(this, options.WithType(typeof(T))));
        }

        public IAsyncQueryable<T> GetQueryable(Action<SPQueryOptions> configure = null)
        {
            var options = new SPQueryOptions(typeof(T));
            configure?.Invoke(options);
            return this.GetQueryable(options);
        }
        private void Copy(T source, ListItem dest)
        {
            var fields = this.SPList.Fields;
            var values = new Dictionary<string, object>(source.FieldValuesEx);
            foreach (var val in values)
            {
                var col = this.GetFieldByName(val.Key);
                if (col != null && !col.ReadOnlyField)
                {
                    dest[val.Key] = val.Value;
                }
            }

        }
        public async Task<T> UpdateItem(T item)
        {
            await this.Load();
            var _item = item.ListItem;
            if (_item != null)
            {
                Copy(item, _item);
                _item.Update();
                await this.Context.ExecuteQueryAsync();
            }
            return item;
        }

        internal string GetColumnName(string propName)
        {
            return typeof(T).GetProperty(propName)?.GetColumnName() ?? propName;
        }
        internal Tuple<string, string> GetFilterVal(string column, object value)
        {
            var fields = this.SPList.Fields;
            //var p = typeof(T).GetProperty(column)?.GetColumnName();
            column = typeof(T).GetProperty(column)?.GetColumnName() ?? column;
            var field = this.GetFieldByName(column);
            switch (field.TypeAsString)
            {
                case "DateTime":
                    {
                        if (value is DateTime dt)
                        {
                            return new Tuple<string, string>(field.TypeAsString, dt.ToUniversalIso8601());
                        }
                        throw new Exception(
                            $"Invalid Filter Value. Value should be DateTime.");
                    }

                default:
                    return new Tuple<string, string>(field.TypeAsString, value.ToString());
            }

            return new Tuple<string, string>("Text", value.ToString());
        }

        public async Task DeleteItems(params T[] args)
        {
            await this.Load();
            foreach (var item in args)
            {
                if (item.ListItem != null)
                {
                    item.ListItem.DeleteObject();
                }
            }
            await this.Context.ExecuteQueryAsync();

        }

        public async Task<T[]> UpdateItems(params T[] items)
        {
            await this.Load();
            var result = new List<T>();
            foreach (var item in items)
            {
                if (item.ListItem != null)
                {
                    Copy(item, item.ListItem);
                    item.ListItem.Update();
                    var _result = Activator.CreateInstance<T>();
                    _result.Init(item.ListItem, this);
                    result.Add(_result);

                }
            }
            await this.Context.ExecuteQueryAsync();
            return result.ToArray();
        }

        public async Task<T[]> InsertItems(params T[] items)
        {
            var result = new List<T>();
            await this.Load();
            foreach (var item in items)
            {
                var _item = this.SPList.AddItem(new ListItemCreationInformation { });
                Copy(item, _item);
                _item.Update();
                var _result = Activator.CreateInstance<T>();
                _result.Init(_item, this);
                result.Add(_result);
            }
            await this.Context.ExecuteQueryAsync();
            return result.ToArray();
        }

        public async Task UploadFile()
        {
            await this.Load();
            this.Context.Load(this.SPList.RootFolder, x => x.Folders, x => x.Files);
            await this.Context.ExecuteQueryAsync();
            var context = this.Context;
            var uploadFolderUrl = "MyDocuments/MyFolder";
            var uploadFilePath = "c:\\temp\\internal-nlog.txt";
            var fileCreationInfo = new FileCreationInformation
            {
                Content = System.IO.File.ReadAllBytes(uploadFilePath),
                Overwrite = true,
                Url = Path.GetFileName(uploadFilePath)
            };
            var targetFolder = context.Web.GetFolderByServerRelativeUrl(uploadFolderUrl);
            var uploadFile = targetFolder.Files.Add(fileCreationInfo);
            context.Load(uploadFile);
            context.ExecuteQuery();

        }

        public async Task<SPFolder> GetRootFolder()
        {
            await this.Load();
            this.Context.Load(this.SPList.RootFolder, x => x.Folders, x => x.Files);
            await this.Context.ExecuteQueryAsync();
            return new SPFolder(this.SPList.RootFolder);
        }

        public async Task<T[]> InsertItemsInFolder(SPFolder folder = null, params T[] items)
        {
            var result = new List<T>();
            await this.Load();
            if (await this.IsDocumetLibrary())
            {
                return await this.InsertDocuments(folder, items);

            }
            foreach (var item in items)
            {
                var _item = this.SPList.AddItem(
                    string.IsNullOrWhiteSpace(folder?.Folder?.ServerRelativeUrl)
                    ? new ListItemCreationInformation { }
                    : new ListItemCreationInformation { FolderUrl = folder?.Folder?.ServerRelativeUrl });
                Copy(item, _item);
                _item.Update();
                var _result = Activator.CreateInstance<T>();
                _result.Init(_item, this);
                result.Add(_result);
            }
            await this.Context.ExecuteQueryAsync();
            return result.ToArray();
        }

        public async Task<T[]> InsertDocuments(SPFolder folder = null, params T[] items)
        {
            var result = new List<T>();
            await this.Load();
            folder = folder ?? await this.GetRootFolder();
            foreach (var item in items.Where(x => x.FileCreationInfo != null))
            {
                var newFile = item.FileCreationInfo;
                //var newFile = new FileCreationInformation
                //{
                //    Url = item.FileInfo.FileName,
                //    Content = await item.FileInfo.GetContent()
                //};
                var file = folder.Folder.Files.Add(item.FileCreationInfo);
                var _item = file.ListItemAllFields;
                Copy(item, _item);
                _item.Update();
                var _result = Activator.CreateInstance<T>();
                _result.Init(_item, this);
                result.Add(_result);
            }
            foreach (var i in result)
            {
                this.Context.Load(i.ListItem, x => x.Id);
            }
            await this.Context.ExecuteQueryAsync();
            return result.ToArray();


        }

        public async Task<T> GetItemById(int id)
        {
            await this.Load();
            var r = this.SPList.GetItemById(id);
            this.Context.Load(r);
            await this.Context.ExecuteQueryAsync();
            var result = Activator.CreateInstance<T>();
            result.Init(r, this);
            return result;
        }
    }
}
