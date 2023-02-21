using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.SharePoint.Internals
{
    public class ListEx : List
    {
        public ListEx(ClientRuntimeContext context, ObjectPath objectPath) : base(context, objectPath)
        {
        }

    }
    public class ListEx<T> : ListEx where T : SPItem
    {
        public ListEx(ClientRuntimeContext context, ObjectPath objectPath) : base(context, objectPath)
        {
        }
    }
}
