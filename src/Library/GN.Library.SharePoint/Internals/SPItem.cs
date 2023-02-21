using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.SharePoint.Internals
{
    public class SPItem
    {

        public class Schema
        {
            public const string Title = "Title";
            public const string Id = "ID";
            public const string Created = "Created";
            public const string Modified = "Modified";
            public const string FileDirRef = "FileDirRef";
            public const string FileLeafRef = "FileLeafRef";
        }
        protected ListItem _item;
        //protected SPListAdapter adapter;
        private Dictionary<string, object> fieldValues;
        public ClientContext Context => this._item.Context.Extend();
        public ListItem ListItem => _item;

        public FileCreationInformationEx FileCreationInfo { get; private set; }
        //public AttachmentCreationInformationEx AttachmentCreationInfo { get; private set; }
        public SPItem()
        {
            
        }
        [Column(Schema.Title)]
        public string Title { get => GetAttibuteValue<string>(Schema.Title); set => this.SetAttributeValue(Schema.Title, value); }

        [Column(Schema.Id)]
        public int Id => this._item?.Id ?? 0;

        [Column(Schema.Created)]
        public DateTime Created => GetAttibuteValue<DateTime>(Schema.Created);

        [Column(Schema.Modified)]
        public DateTime Modifield => GetAttibuteValue<DateTime>(Schema.Modified);

        [Column(Schema.FileDirRef)]
        public string FileDirRef => GetAttibuteValue<string>(Schema.FileDirRef);

        [Column(Schema.FileLeafRef)]
        public string FileLeafRef => GetAttibuteValue<string>(Schema.FileLeafRef);

        public SPItem Init(ListItem item, SPListAdapter adapter = null)
        {
            this._item = item;
            return this;
        }
        public Dictionary<string, object> FieldValuesEx
        {
            get
            {
                this.fieldValues = this.fieldValues ?? new Dictionary<string, object>();
                return this._item == null
                    ? this.fieldValues
                    : this._item.FieldValues;
            }
        }
        public void SetAttributeValue(string name, object value)
        {
            this.FieldValuesEx[name] = value;
        }
        public T GetAttibuteValue<T>(string name)
        {
            if (this.FieldValuesEx.ContainsKey(name))
                return (T)this.FieldValuesEx[name];
            return default(T);
        }
        private bool IsUpdateable(string fName)
        {
            return true;
        }
        public void CopyValuesTo(ListItem item)
        {
            var f = new Dictionary<string, object>(this.FieldValuesEx);
            foreach (var val in f)
            {
                item[val.Key] = val.Value;
            }
        }

        public FileSystemObjectType FileSystemObjectType => this.ListItem.FileSystemObjectType;
        public SPItem SetFile(Action<FileCreationInformationEx> configure)
        {
            var info = new FileCreationInformationEx();
            configure?.Invoke(info);
            this.FileCreationInfo = info;
            return this;
        }

        
        public async Task<FileInformation> GetFile()
        {
            this.Context.Load(_item.File);
            await this.Context.ExecuteQueryAsync();
            var fileRef = _item.File.ServerRelativeUrl;
            return Microsoft.SharePoint.Client.File.OpenBinaryDirect(this.Context, fileRef);
        }
        public async Task<byte[]> GetFileContent()
        {
            var file = await this.GetFile();
            var mem = new MemoryStream();
            await file.Stream.CopyToAsync(mem);
            return mem.ToArray();
        }
        //public async Task<SPFolder> GetFolder()
        //{
        //    this.Context.Load(_item.Folder, x=>x.Name);
        //    await this.Context.ExecuteQueryAsync();
        //    return new SPFolder(_item.Folder);
        //}

        public T To<T>() where T : SPItem
        {
            var result = Activator.CreateInstance<T>();
            result.Init(this.ListItem);
            return result;
        }
        public object To(Type type)
        {
            var result =(SPItem) Activator.CreateInstance(type);
            result.Init(this.ListItem);
            return result;

        }
    }
}
