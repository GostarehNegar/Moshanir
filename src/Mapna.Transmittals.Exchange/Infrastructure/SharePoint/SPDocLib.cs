using GN.Library.SharePoint.Internals;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mapna.Transmittals.Exchange.Internals
{
    public class SPDocLib : ListEx<SPDocLibItem>
    {
        public SPDocLib(ClientRuntimeContext context, ObjectPath objectPath) : base(context, objectPath)
        {
        }
    }
}
