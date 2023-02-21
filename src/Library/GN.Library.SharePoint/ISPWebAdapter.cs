using GN.Library.SharePoint.Internals;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.SharePoint
{
    public interface ISPWebAdapter
    {
        ClientContextExEx Context { get; }
        void Initialize(ClientContextExEx context);
        void Test();
        Task<IEnumerable<ISPListAdapter>> GetLists(bool refersh = false);
        Task<ISPListAdapter> GetListByDefaultUrl(string path, bool refersh = false);
        Task<ISPListAdapter<T>> GetListByDefaultUrl<T>(string path, bool refersh = false) where T : SPItem;
        Task<bool> EnsureConnection(bool ThrowIfFailed = true);
        Task<string> Title { get; }
    }
}
