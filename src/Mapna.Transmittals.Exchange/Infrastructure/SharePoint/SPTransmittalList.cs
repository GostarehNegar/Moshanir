using GN.Library.SharePoint.Internals;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mapna.Transmittals.Exchange.Internals
{
    class SPTransmittalList : ListEx<SPTransmittalItem>
    {
        public SPTransmittalList(ClientRuntimeContext context, ObjectPath objectPath) : base(context, objectPath)
        {
        }
    }
}
