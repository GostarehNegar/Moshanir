
using GN.Library.SharePoint.Internals;
using GN.Library.SharePoint.SP2010;
using GN.Library.SharePoint.SP2010.WebReferences.Lists;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace GN.Library.SharePoint
{
    public static partial class SharePointExtensions
    {
        public static IServiceCollection AddSharePointServices(this IServiceCollection services, IConfiguration configuration, Action<SharePointOptions> configure = null)
        {
            var options = new SharePointOptions();
            options.ConnectionString = configuration?.GetConnectionString("wss");
            configure?.Invoke(options);
            services.AddSingleton(options);
            services.AddTransient<ISPWebFactory, SPWeFactory>();
            services.AddTransient<IClientContextFactory, ClientContextFactory>();
            return services;


        }

        public static Stream OpenFileByUrl(this ClientContext ctx,string file )
        {
            return Microsoft.SharePoint.Client.File.OpenBinaryDirect(ctx, file).Stream;
            
            
        }
        public static string ToUniversalIso8601(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("u").Replace(" ", "T");
        }
        internal static ColumnAttribute GetColumnAttribute(this PropertyInfo property)
        {
            return property.GetCustomAttribute<ColumnAttribute>(true);
        }
        internal static string GetColumnName(this PropertyInfo property)
        {
            return property.GetColumnAttribute()?.Name;
        }
        internal static ClientContextExEx Extend_Deprecated(this ClientRuntimeContext ctx)
        {
            var result = ctx as ClientContextExEx;
            if (result == null)
            {
                throw new Exception("Invalid Context!!!");
            }

            return result;
        }
        public static ClientContext Extend(this ClientRuntimeContext ctx)
        {
            var result = ctx as ClientContext;
            if (result == null)
            {
                throw new Exception("Invalid Context!!!");
            }

            return result;
        }

        internal static IEnumerable<KeyValuePair<string, string>> ParseConnectionString(string connectionString)
        {
            var result = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                foreach (var item in connectionString.Split(';'))
                {
                    var keyValueArray = item.Split('=');
                    var key = keyValueArray[0];
                    var val = keyValueArray.Length > 1 ? keyValueArray[1] : null;
                    if (!string.IsNullOrEmpty(key))
                        result.Add(new KeyValuePair<string, string>(key.Trim(), val?.Trim()));
                }
            }
            return result;
        }
        public static void Test()
        {
            var s = new Lists();
            s.Url = "http://projects.gnco.ir/parnian/_vti_bin/Lists.asmx";
            s.UseDefaultCredentials = true;
            var f = s.GetListCollection();
            var adapter = new GN.Library.SharePoint.SP2010.WebReferences.Webs.Webs();
            adapter.Url = "http://projects.gnco.ir/parnian/_vti_bin/Webs.asmx";
            adapter.UseDefaultCredentials = true;
            var webs = adapter.GetWebCollection();
            var ctx = new ClientContext("http://projects.gnco.ir");
            ctx.Credentials = CredentialCache.DefaultNetworkCredentials;
            var web = ctx.Web;
            ctx.ExecuteQuery();
            ctx.Load(web.Lists, lists => lists.Include(x => x.DefaultViewUrl, x => x.Title));
            ctx.ExecuteQuery();




        }

        public static T Extend<TSource, T>(this TSource source) where T : TSource where TSource : ClientObject
        {

            return (T)Activator.CreateInstance(typeof(T), source.Context, source.Path);
        }
        public static T Extend<T>(this Web source) where T : Web => source.Extend<Web, T>();
        public static T Extend<T>(this List source) where T : List => source.Extend<List, T>();

        public static async Task<SPListAdapter<T>> GetAdapter<T>(this Web web, string path) where T : SPItem
        {
            var lists = await web.With(w => w.Lists,
                 w => w.Lists.Include(l => l.DefaultViewUrl, l => l.Title))
                .DoAsync(x => x.Lists);
            var list = lists.FirstOrDefault(x => x.DefaultViewUrl.ToLowerInvariant().Contains(path.ToLowerInvariant()));
            return new SPListAdapter<T>(list);
        }
        public static async Task<List> GetListByPath(this Web web, string path)
        {
            var lists = await web.With(w => w.Lists,
                 w => w.Lists.Include(l => l.DefaultViewUrl, l => l.Title))
                .DoAsync(x => x.Lists);
            return lists.FirstOrDefault(x => x.DefaultViewUrl.ToLowerInvariant().Contains(path.ToLowerInvariant()));
            //return new SPListAdapter<T>(list);
        }
        internal static string GetColumnName(this Type type, string propName)
        {
            return type.GetProperty(propName)?.GetColumnName() ?? propName;
        }

        internal static Tuple<string, string> GetFilterVal(this List list, string column, object value)
        {
            //var fields = this.SPList.Fields;
            //var p = typeof(T).GetProperty(column)?.GetColumnName();
            //column = typeof(T).GetProperty(column)?.GetColumnName() ?? column;
            var field = list.GetFieldByName(column);
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
        }
        public static Task<Microsoft.SharePoint.Client.File> SaveBinaryDirectAsync(this ClientContext context, string relativePath, Stream stream, bool @override = true)
        {
            return Task<Microsoft.SharePoint.Client.File>.Run(() =>
            {
                Microsoft.SharePoint.Client
                   .File.SaveBinaryDirect(context, relativePath, stream, @override);
                return context.Web.GetFileByServerRelativeUrl(relativePath);
            });

        }
        public static async Task UploadFile(this Web web, string relativePath, Stream stream)
        {

            await web.With(x => x.ServerRelativeUrl)
                .DoAsync(w =>
                {
                    var mm = new MemoryStream();
                    var context = web.Context as ClientContext;
                    var uploadFolderUrl = "MyDocuments";
                    var uploadFilePath = "c:\\temp\\ngrok.mmm";
                    //var stream = new FileStream(uploadFilePath, FileMode.Open);
                    //var fileCreationInfo = new FileCreationInformation
                    //{
                    //    Content = System.IO.File.ReadAllBytes(uploadFilePath),
                    //    //ContentStream = stream,
                    //    Overwrite = true,
                    //    Url = Path.GetFileName(uploadFilePath)
                    //};
                    var targetFolder = web.GetFolderByServerRelativeUrl(uploadFolderUrl);

                    targetFolder.With(x => x.ServerRelativeUrl).Do(x => { });
                    //var uploadFile = targetFolder.Files.Add(fileCreationInfo);
                    Microsoft.SharePoint.Client
                    .File.SaveBinaryDirect(context, targetFolder.ServerRelativeUrl + "/ngrok.mmm", stream, true);

                    //context.Load(uploadFile);
                    //context.ExecuteQuery();
                });
        }
        public static async Task UploadFileWithStream(this Web web)
        {

            await web.With(x => x.ServerRelativeUrl)
                .DoAsync(w =>
                {
                    var mm = new MemoryStream();
                    var context = web.Context as ClientContext;
                    var uploadFolderUrl = "MyDocuments";
                    var uploadFilePath = "c:\\temp\\1.jpg";
                    var stream = new FileStream(uploadFilePath, FileMode.Open);
                    var fileCreationInfo = new FileCreationInformation
                    {
                        //Content = System.IO.File.ReadAllBytes(uploadFilePath),
                        ContentStream = stream,
                        Overwrite = true,
                        Url = Path.GetFileName(uploadFilePath)
                    };
                    var targetFolder = web.GetFolderByServerRelativeUrl(uploadFolderUrl);
                    var uploadFile = targetFolder.Files.Add(fileCreationInfo);
                    context.Load(uploadFile);
                    context.ExecuteQuery();
                });
        }
       

        public static async Task AttachByWebService(this ListItem item, string fileName, byte[] content)
        {
            await item.With(x => x.ParentList).DoAsync(x =>
            {
                using(var _lists = new SP2010.WebReferences.Lists.Lists())
                {
                    //var ctx = $"{item.Context.Url}/_vti_bin/Lists.asmx";
                    _lists.Url = $"{item.Context.Url}/_vti_bin/Lists.asmx";
                    _lists.Credentials = item.Context.Credentials;

                    _lists.AddAttachment(x.ParentList.Id.ToString(), item.Id.ToString(), fileName, content) ;
                }
            });
        }
        public static async Task AttachByWebService(this List list, int itemid, string fileName, byte[] content)
        {
            await list.With(x => x.Id).DoAsync(x =>
            {
                using (var _lists = new SP2010.WebReferences.Lists.Lists())
                {
                    //var ctx = $"{item.Context.Url}/_vti_bin/Lists.asmx";
                    _lists.Url = $"{list.Context.Url}/_vti_bin/Lists.asmx";
                    _lists.Credentials = list.Context.Credentials;

                    _lists.AddAttachment(x.Id.ToString(), itemid.ToString(), fileName, content);
                }
            });
        }

    }
}
