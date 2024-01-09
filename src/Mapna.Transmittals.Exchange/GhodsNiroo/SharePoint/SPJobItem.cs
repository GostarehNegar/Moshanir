using GN.Library.SharePoint.Internals;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mapna.Transmittals.Exchange.GhodsNiroo.SharePoint
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
        public string SourceId { get => GetAttibuteValue<string>(Schema.SourceId); set => SetAttributeValue(Schema.SourceId, value); }

        [Column(Schema.InternalId)]
        public string InternalId { get => GetAttibuteValue<string>(Schema.InternalId); set => SetAttributeValue(Schema.InternalId, value); }

        [Column(Schema.Status)]
        public string Status { get => GetAttibuteValue<string>(Schema.Status); set => SetAttributeValue(Schema.Status, value); }

        [Column(Schema.StatusReason)]
        public string StatusReason { get => GetAttibuteValue<string>(Schema.StatusReason); set => SetAttributeValue(Schema.StatusReason, value); }


        [Column(Schema.Content)]
        public string Content { get => GetAttibuteValue<string>(Schema.Content); set => SetAttributeValue(Schema.Content, value); }

        [Column(Schema.State)]
        public string State { get => GetAttibuteValue<string>(Schema.State); set => SetAttributeValue(Schema.State, value); }

        [Column(Schema.Direction)]
        public string Direction { get => GetAttibuteValue<string>(Schema.Direction); set => SetAttributeValue(Schema.Direction, value); }


        //public TransmittalSubmitModel GetTransmittal()
        //{
        //    return MapnaTransmittalsExtensions.TryDeserialize<TransmittalSubmitModel>(Content, out var result) ? result : null;
        //}

        public void SetCompleted()
        {
            Status = Schema.Statuses.Completed;
        }
        public override string ToString()
        {
            return $"{Title}";
        }
    }
}
