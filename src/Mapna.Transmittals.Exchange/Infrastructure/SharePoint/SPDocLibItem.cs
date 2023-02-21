using GN.Library.SharePoint.Internals;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mapna.Transmittals.Exchange.Internals
{
    public class SPDocLibItem : SPItem
    {
        public new class Schema : SPItem.Schema
        {
            public const string Transmittal = "TrNoLook";
            public const string DocumentNumber = "DocNoLook";
            public const string Purpose = "Pur";
            public const string Status = "Stat";
            public const string IntRev = "IntRev";
            public const string ExtRev ="ExtRev";
        }

        [Column(Schema.Transmittal)]
        public int? TransmittalId
        {
            get => this.GetAttibuteValue<FieldLookupValue>(Schema.Transmittal)?.LookupId;
            set => this.SetAttributeValue(Schema.Transmittal, value);
        }

        [Column(Schema.Transmittal)]
        public string Transmittal { get => this.GetAttibuteValue<FieldLookupValue>(Schema.Transmittal)?.LookupValue; }

        [Column(Schema.DocumentNumber)]
        public int? DocumentNumberId
        {
            get => this.GetAttibuteValue<FieldLookupValue>(Schema.DocumentNumber)?.LookupId;
            set => this.SetAttributeValue(Schema.DocumentNumber, value);
        }



        [Column(Schema.Purpose)]
        public string Purpose
        {
            get => this.GetAttibuteValue<string>(Schema.Purpose);
            set => this.SetAttributeValue(Schema.Purpose, value);
        }

        [Column(Schema.Status)]
        public string Status
        {
            get => this.GetAttibuteValue<string>(Schema.Status);
            set => this.SetAttributeValue(Schema.Status, value);
        }

        [Column(Schema.IntRev)]
        public string IntRev
        {
            get => this.GetAttibuteValue<string>(Schema.IntRev);
            set => this.SetAttributeValue(Schema.IntRev, value);
        }

        [Column(Schema.ExtRev)]
        public string ExtRev
        {
            get => this.GetAttibuteValue<string>(Schema.ExtRev);
            set => this.SetAttributeValue(Schema.ExtRev, value);
        }

    }
}
