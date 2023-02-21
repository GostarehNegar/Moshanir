using GN.Library.SharePoint.Internals;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mapna.Transmittals.Exchange.Internals
{
    public class SPMasterListItem : SPItem
    {
        public new class Schema : SPItem.Schema
        {
            public const string DocumentNumber = "DocNoSi";
            public const string DocumentTitle = "Title";
        }
        [Column(Schema.DocumentNumber)]
        public string DocumentNumber
        {
            get => this.GetAttibuteValue<string>(Schema.DocumentNumber);
            set => this.SetAttributeValue(Schema.DocumentNumber, value);
        }
        [Column(Schema.DocumentTitle)]
        public string DocumentTitle
        {
            get => this.GetAttibuteValue<string>(Schema.DocumentTitle);
            set => this.SetAttributeValue(Schema.DocumentTitle, value);
        }

    }
}
