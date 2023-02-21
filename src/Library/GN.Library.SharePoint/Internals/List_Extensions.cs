using GN.Library.SharePoint.Internals;
using GN.Library.SharePoint.Internals.LinqQuery;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.SharePoint
{
    public static partial class SPListExtensions
    {
        public static Field GetFieldByName(this List list, string name, bool ThrowIfNotFound = true)
        {
            var result = list
                 .With(x => x.Fields)
                 .Add(x => x.Fields.Include(f => f.Title, f => f.InternalName, f => f.FieldTypeKind, f => f.TypeAsString, f => f.ReadOnlyField))
                 .Do(x =>
                 {
                     return x.Fields.FirstOrDefault(f1 => f1.InternalName == name);
                 }
                 );
            if (result == null && ThrowIfNotFound)
            {
                throw new Exception(
                       $"Field Not Found. '{name}'");
            }
            return result;
        }
        public static bool IsUpdateable(this Field field)
        {
            string[] SpecialFileds = new string[] { "Attachments" };
            return field != null && !field.ReadOnlyField && !SpecialFileds.Any(x => x == field.InternalName);
        }
        public static void Copy(this List list, SPItem source, ListItem dest)
        {
            //var fields = this.SPList.Fields;
            var values = new Dictionary<string, object>(source.FieldValuesEx);
            foreach (var val in values)
            {
                var col = list.GetFieldByName(val.Key);

                if (col != null && col.IsUpdateable())
                {
                    dest[val.Key] = val.Value;
                }
            }
        }

        public static async Task DeleteItem<T>(this ListEx<T> list, T item) where T : SPItem
        {
            await list.DeleteItems(new T[] { item });
        }
        public static async Task<T> UpdateItem<T>(this ListEx<T> list, T item) where T : SPItem
        {
            await list.Load();
            var _item = item.ListItem;
            if (_item != null)
            {
                list.Copy(item, _item);
                _item.Update();
                await list.Context.ExecuteQueryAsync();
            }
            return item;
        }
        public static async Task<T> InsertItem<T>(this ListEx<T> list, T item) where T : SPItem
        {
            var result = await list.InsertItems<T>(new T[] { item });
            return result.FirstOrDefault();

        }
        public static async Task<T> InsertItem<T>(this List list, T item) where T : SPItem
        {
            var result = await list.InsertItems<T>(new T[] { item });
            return result.FirstOrDefault();

        }
        public static Task<T[]> InsertItems<T>(this ListEx<T> list, params T[] items) where T : SPItem
        {
            return InsertItems((List)list, items);
        }

        //public static Task<SPItem[]> InsertItems(this List list, params SPItem[] items) => InsertItems<SPItem>(list, items);
        public static async Task<T[]> InsertItems<T>(this List list, params T[] items) where T : SPItem
        {
            var result = new List<T>();
            await list.Load();
            if (await list.IsDocumetLibrary())
            {
                return await list.InsertDocuments(null, items);

            }
            foreach (var item in items)
            {
                var _item = list.AddItem(new ListItemCreationInformation { });
                Copy(list, item, _item);
                _item.Update();
                var _result = Activator.CreateInstance<T>();
                _result.Init(_item, null);
                result.Add(_result);
            }
            await list.Context.ExecuteQueryAsync();
            return result.ToArray();
        }
        internal static async Task<bool> ValidateQuery(this List list, SPQueryOptions query)
        {
            await list.With(x => x.Fields.Include(f => f.Title, f => f.InternalName, f => f.FieldTypeKind, f => f.TypeAsString, f => f.ReadOnlyField))
                .DoAsync(x => true);
            if (query.Columns != null)
            {
                var non_exisiting = query.Columns.FirstOrDefault(x => list.GetFieldByName(x) == null);
            }
            return true;
        }
        public static async ValueTask<bool> IsDocumetLibrary(this List list)
        {
            return await list
                .With(x => x.BaseType)
                .DoAsync(x => x.BaseType == BaseType.DocumentLibrary);
        }
        public static async Task Load(this List list, bool refersh = false)
        {
            await list.With(l => l.Fields, l => l.Fields.Include(f => f.Title, f => f.InternalName, f => f.FieldTypeKind, f => f.TypeAsString, f => f.ReadOnlyField)).DoAsync(x => true);
        }
        public static async Task<T[]> InsertItemsInFolder<T>(this List list, Folder folder = null, params T[] items) where T : SPItem
        {
            var result = new List<T>();
            await list.Load();
            if (await list.IsDocumetLibrary())
            {
                return await list.InsertDocuments(folder, items);

            }
            foreach (var item in items)
            {
                var _item = list.AddItem(
                    string.IsNullOrWhiteSpace(folder?.ServerRelativeUrl)
                    ? new ListItemCreationInformation { }
                    : new ListItemCreationInformation { FolderUrl = folder?.ServerRelativeUrl });
                Copy(list, item, _item);
                _item.Update();
                var _result = Activator.CreateInstance<T>();
                _result.Init(_item, null);
                result.Add(_result);
            }
            await list.Context.ExecuteQueryAsync();
            return result.ToArray();
        }
        public static Task<Microsoft.SharePoint.Client.File[]> GetAttachments(this SPItem item) =>
            item.ListItem.GetAttachments();
        public static async Task<Microsoft.SharePoint.Client.File[]> GetAttachments(this ListItem item)
        {
            try
            {
                var files = await item.ParentList.With(l => l.RootFolder)
                    .DoAsync(async list =>
                    {
                        return await list.ParentWeb
                            .GetFolderByServerRelativeUrl($"{list.RootFolder.ServerRelativeUrl}/Attachments/{item.Id}")
                            .With(x => x.Files)
                            .DoAsync(f => f.Files);
                    });
                return files
                    .Select(x => x)
                    .ToArray();
            }
            catch (ServerException err) when (err.ServerErrorTypeName == typeof(FileNotFoundException).FullName)
            {
                return new Microsoft.SharePoint.Client.File[] { };
            }
        }
        public static async Task<Folder> GetRootFolder(this List list)
        {
            var _f = await list.With(l => l.RootFolder).DoAsync(l =>
            {
                return l.RootFolder.With(f => f.Files, f => f.Folders).DoAsync(f => f);
            });
            return _f;




            //await this.Load();
            //this.Context.Load(this.SPList.RootFolder, x => x.Folders, x => x.Files);
            //await this.Context.ExecuteQueryAsync();
            //return new SPFolder(this.SPList.RootFolder);
        }
        public static async Task<T[]> InsertDocuments<T>(this List list, Folder folder = null, params T[] items) where T : SPItem
        {
            var result = new List<T>();
            await list.Load();
            folder = folder ?? await list.GetRootFolder();
            foreach (var item in items.Where(x => x.FileCreationInfo != null))
            {
                var newFile = item.FileCreationInfo;
                //var newFile = new FileCreationInformation
                //{
                //    Url = item.FileInfo.FileName,
                //    Content = await item.FileInfo.GetContent()
                //};
                var file = folder.Files.Add(item.FileCreationInfo);
                var _item = file.ListItemAllFields;
                Copy(list, item, _item);
                _item.Update();
                var _result = Activator.CreateInstance<T>();
                _result.Init(_item, null);
                result.Add(_result);
            }
            foreach (var i in result)
            {
                list.Context.Load(i.ListItem, x => x.Id);
            }
            await list.Context.ExecuteQueryAsync();
            return result.ToArray();
        }
        public static IAsyncQueryable<T> GetQueryable<T>(this List list, Action<SPQueryOptions> configure = null) where T : SPItem
        {
            var options = new SPQueryOptions().WithType(typeof(T));
            configure?.Invoke(options);
            //await list.Load();
            return new Queryable<T>(new QueryExecutor<T>(list, options.WithType(typeof(T))));
        }
        public static IAsyncQueryable<T> GetQueryable<T>(this ListEx<T> list, Action<SPQueryOptions> configure = null) where T : SPItem
        {
            var options = new SPQueryOptions().WithType(typeof(T));
            configure?.Invoke(options);
            //await list.Load();
            return new Queryable<T>(new QueryExecutor<T>(list, options.WithType(typeof(T))));
        }
        public static async Task<T> GetItemById<T>(this List list, int id, bool ThrowEIfNotFound = false) where T : SPItem
        {
            await list.Load();
            var r = list.GetItemById(id);
            list.Context.Load(r);
            try
            {
                await list.Context.ExecuteQueryAsync();
                var result = Activator.CreateInstance<T>();
                result.Init(r, null);
                return result;
            }
            catch (ServerException err)
            {
                if (ThrowEIfNotFound)
                {
                    throw err;
                }
            }
            return null;
        }
        public static async Task DeleteItems<T>(this List list, params T[] args) where T : SPItem
        {
            await list.Load();
            foreach (var item in args)
            {
                if (item.ListItem != null)
                {
                    item.ListItem.DeleteObject();
                }
            }
            await list.Context.ExecuteQueryAsync();

        }

        public static async Task<Attachment> Attach(this ListItem item, string fileName, byte[] content)
        {
            //List list = web.Lists.GetByTitle("listtitle");
            //ListItem item = list.GetItemById(1);
            var attachment = new AttachmentCreationInformation();
            item.Context.Load(item.AttachmentFiles);
            item.Context.ExecuteQuery();
            attachment.FileName = fileName;
            attachment.ContentStream = new MemoryStream(content);
            attachment.ContentStream.Seek(0, SeekOrigin.Begin);
            var result = item.AttachmentFiles.Add(attachment);
            item.Context.Load(result);
            item.Update();
            //item.Context.Load(result);
            await item.Context.ExecuteQueryAsync();
            return result;
        }
    }

}
