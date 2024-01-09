using GN.Library.SharePoint.Internals;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Text;
using GN.Library.SharePoint;
using System.Linq;

namespace Mapna.Transmittals.Exchange.Internals
{
    class SPTransmittalList : ListEx<SPTransmittalItem>
    {
        static IEnumerable<ContentType> contentTypes;
        public SPTransmittalList(ClientRuntimeContext context, ObjectPath objectPath) : base(context, objectPath)
        {
        }

        public IEnumerable<ContentType> GetContentTypes(bool refersh = false)
        {
            if (contentTypes==null || refersh)
            {
                contentTypes = this.ContentTypes.WithCollectionEx(x => x.Name, x=>x.Id)
                    .ToArray();
            }
            return contentTypes;
        }

        public ContentType GetContentTypeByName(string name)
        {
            return this.GetContentTypes()
                .FirstOrDefault(x => string.Compare(x.Name, name, true) == 0);
        }

    }


}
