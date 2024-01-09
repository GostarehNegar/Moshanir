using GN.Library.SharePoint.Internals;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.GhodsNiroo.SharePoint
{
    public class SPTransmittalItem: SPItem
    {
        public new class Schema : SPItem.Schema
        {
            public const string ProjectCode = "ProjectCode";
            public const string TransmittalNo = "TransmittalNo";

        }
        [Column(Schema.ProjectCode)]
        public string ProjectCode { get => GetAttibuteValue<string>(Schema.ProjectCode); set => SetAttributeValue(Schema.ProjectCode, value); }

        [Column(Schema.TransmittalNo)]
        public string TransmittalNo { get => GetAttibuteValue<string>(Schema.TransmittalNo); set => SetAttributeValue(Schema.TransmittalNo, value); }

    }
}
