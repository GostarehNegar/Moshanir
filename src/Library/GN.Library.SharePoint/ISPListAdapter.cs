using GN.Library.SharePoint.Internals;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.SharePoint
{
    public interface ISPListAdapter
    {
        List SPList { get; }
        ClientContextExEx Context { get; }
        ISPListAdapter Initialize(List list);
        string DefaultViewUrl { get; }
        Task Test();
        Task<IEnumerable<SPItem>> GetItems();
        With<List> WithList(params Expression<Func<List, object>>[] selector);
        
    }
    public interface ISPListAdapter<T> : ISPListAdapter where T : SPItem
    {
        IAsyncQueryable<T> GetQueryable(SPQueryOptions options);
        IAsyncQueryable<T> GetQueryable(Action<SPQueryOptions> configure = null);
        Task<T> AddItem(T item);
        Task<T> GetItemById(int id);
        Task<T[]> InsertItems(params T[] items);
        Task<T[]> InsertItemsInFolder(SPFolder folder = null, params T[] items);
        Task<T[]> InsertDocuments(SPFolder folder = null, params T[] items);

        Task<T> UpdateItem(T item);
        Task<T[]> UpdateItems(params T[] items);

        Task DeleteItem(T item);
        Task DeleteItems(params T[] args);
        Task UploadFile();
        Task<SPFolder> GetRootFolder();
    }
}
