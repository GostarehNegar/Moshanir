using GN.Library.SharePoint.Internals;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mapna.Transmittals.Exchange.Internals
{
    public interface ITransmittalData
    {

    }
    public class SPTransmittalItem : SPItem, ITransmittalData
    {
        public new class Schema : SPItem.Schema
        {
            //public const string ReferenceNumber = "ReferenceNumber";
            public const string TransmittalNo = "TrNoSi";
            public const string LetterNo = "TrLetterNo";
            public const string TrAction = "TrAction";
            public const string From = "FromSi";
            public const string ToLook = "ToLook";
            public const string ToSi = "ToSi";
            public const string DiscFirstLook0 = "DiscFirstLook0";
            public const string TrDateHijri = "TrDateHijri";
            public const string IssueState = "IssueState";
            public const string SendFormal = "SendFormal";

            public enum IssueStates
            {
                Accept ,
                Preparing ,
                Reject ,
                Waiting 
            }
            public static string[] DefaultFields => new string[] {
                ToLook,From,LetterNo,TransmittalNo,ToSi,LetterNo,"CcSi",DiscFirstLook0,TrDateHijri
            };
        }

        public string ReferenceNumber
        {
            get => this.LetterNo;
            set => this.LetterNo = value;
        }

        [Column(Schema.TransmittalNo)]
        public string TransmittalNo { get => this.GetAttibuteValue<string>(Schema.TransmittalNo); set => this.SetAttributeValue(Schema.TransmittalNo, value); }

        [Column(Schema.LetterNo)]
        public string LetterNo { get => this.GetAttibuteValue<string>(Schema.LetterNo); set => this.SetAttributeValue(Schema.LetterNo, value); }

        public string TransmittalTitle
        {
            get => this.Title;
            set => this.Title = value;
        }

        [Column(Schema.TrAction)]
        public string TrAction { get => this.GetAttibuteValue<string>(Schema.TrAction); set => this.SetAttributeValue(Schema.TrAction, value); }

        [Column(Schema.From)]
        public string From { get => this.GetAttibuteValue<string>(Schema.From); set => this.SetAttributeValue(Schema.From, value); }

        [Column(Schema.ToLook)]
        public FieldLookupValue ToLook { get => this.GetAttibuteValue<FieldLookupValue>(Schema.ToLook); set => this.SetAttributeValue(Schema.ToLook, value); }

        [Column(Schema.DiscFirstLook0)]
        public FieldLookupValue DiscFirstLook0
        {
            get => this.GetAttibuteValue<FieldLookupValue>(Schema.DiscFirstLook0);
            set => this.SetAttributeValue(Schema.DiscFirstLook0, value);
        }

        [Column(Schema.ToSi)]
        public string ToSI { get => this.GetAttibuteValue<string>(Schema.ToSi); set => this.SetAttributeValue(Schema.ToSi, value); }

        [Column(Schema.IssueState)]
        public string IssueState { get => this.GetAttibuteValue<string>(Schema.IssueState); set => this.SetAttributeValue(Schema.IssueState, value); }

        [Column(Schema.SendFormal)]
        public string SendFormal { get => this.GetAttibuteValue<string>(Schema.SendFormal); set => this.SetAttributeValue(Schema.SendFormal, value); }


        [Column(Schema.TrDateHijri)]
        public DateTime? TrDateHijri
        {
            get => this.GetAttibuteValue<DateTime?>(Schema.TrDateHijri);
            set => this.SetAttributeValue(Schema.TrDateHijri, value);
        }

        public File[] GetAttachments()
        {
            var files = GN.Library.SharePoint.SPListExtensions.GetAttachments(this.ListItem).Result;
            return files == null || files.Length == 0 ? new File[] { } : files;
        }

    }
}
