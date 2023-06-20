using GN.Library.SharePoint.Internals;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mapna.Transmittals.Exchange.Internals
{
    public class SPJobItem : SPItem
    {
        public new class Schema : SPItem.Schema
        {
            public const string SourceId = "SourceId";
            public const string Status = "Status";
            public const string StatusReason = "StatusReason";
            public const string Content = "JsonContent";
            public const string State = "State";
            public const string InternalId = "InternalId";
            public const string Direction = "Direction";

            public class Statuses
            {
                public const string InProgress = "In Progress";
                public const string Completed = "Completed";
                public const string Failed = "Failed";
                public const string Waiting = "Waiting";


            }
        }

        [Column(Schema.SourceId)]
        public string SourceId { get => this.GetAttibuteValue<string>(Schema.SourceId); set => this.SetAttributeValue(Schema.SourceId, value); }

        [Column(Schema.InternalId)]
        public string InternalId { get => this.GetAttibuteValue<string>(Schema.InternalId); set => this.SetAttributeValue(Schema.InternalId, value); }

        [Column(Schema.Status)]
        public string Status { get => this.GetAttibuteValue<string>(Schema.Status); set => this.SetAttributeValue(Schema.Status, value); }

        [Column(Schema.StatusReason)]
        public string StatusReason { get => this.GetAttibuteValue<string>(Schema.StatusReason); set => this.SetAttributeValue(Schema.StatusReason, value); }


        [Column(Schema.Content)]
        public string Content { get => this.GetAttibuteValue<string>(Schema.Content); set => this.SetAttributeValue(Schema.Content, value); }

        [Column(Schema.State)]
        public string State { get => this.GetAttibuteValue<string>(Schema.State); set => this.SetAttributeValue(Schema.State, value); }

        [Column(Schema.Direction)]
        public string Direction { get => this.GetAttibuteValue<string>(Schema.Direction); set => this.SetAttributeValue(Schema.Direction, value); }


        public TransmittalSubmitModel GetTransmittal()
        {
            return MapnaTransmittalsExtensions.TryDeserialize<TransmittalSubmitModel>(this.Content, out var result) ? result : null;
        }
   
        public void SetCompleted()
        {
            this.Status = Schema.Statuses.Completed;
        }
        public override string ToString()
        {
            return $"{this.Title}";
        }
    }
}
