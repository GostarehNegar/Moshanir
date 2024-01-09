using GN.Library.SharePoint.Internals;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mapna.Transmittals.Exchange.GhodsNiroo.SharePoint
{
    public class SPDocLibItem : SPItem
    {
        public new class Schema : SPItem.Schema
        {
            public const string TransmittalNo = "TransmittalNo";
            public const string DocumentNumber = "DocNoLook";
            public const string Purpose = "Purpose";
            public const string Status = "Status";
            public const string Inhouse = "Inhouse";
            public const string Revision = "Revision";
        }

        [Column(Schema.TransmittalNo)]
        public string TransmittalNo
        {
            get => GetAttibuteValue<string>(Schema.TransmittalNo);
            set => SetAttributeValue(Schema.TransmittalNo, value);
        }

        public string DocumentNumber => GetAttibuteValue<FieldLookupValue>(Schema.DocumentNumber)?.LookupValue;


        [Column(Schema.Purpose)]
        public string Purpose
        {
            get => GetAttibuteValue<string>(Schema.Purpose);
            set => SetAttributeValue(Schema.Purpose, value);
        }

        [Column(Schema.Status)]
        public string Status
        {
            get => GetAttibuteValue<string>(Schema.Status);
            set => SetAttributeValue(Schema.Status, value);
        }

        [Column(Schema.Revision)]
        public string Revision
        {
            get => GetAttibuteValue<string>(Schema.Revision);
            set => SetAttributeValue(Schema.Revision, value);
        }

        [Column(Schema.Inhouse)]
        public string Inhouse
        {
            get => GetAttibuteValue<string>(Schema.Inhouse);
            set => SetAttributeValue(Schema.Inhouse, value);
        }

    }
}
