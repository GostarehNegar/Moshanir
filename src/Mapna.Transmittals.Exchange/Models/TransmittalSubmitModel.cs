﻿using System;
using System.Linq;

namespace Mapna.Transmittals.Exchange
{
    public class TransmittalSubmitModel
    {
        public string Url { get; set; }
        public string ReferedTo { get; set; }
        public string TR_NO { get; set; }
        public string TR_ACTION { get; set; }
        public string Title { get; set; }
        public TransmittalFileSubmitModel[] Documents { get; set; }

        public string GetInternalId() => this.TR_NO;

        public override string ToString()
        {
            return $"{TR_NO}";
        }
        public TransmittalSubmitModel Validate()
        {
            void assert(bool predicate, Func<string> message)
            {
                if (!predicate)
                    throw new ValidationException($"Invalid Transmittal: {this}. {message()}", false);
            }

            if (this.Title!=null && this.Title.Length >= 100)
            {
                this.Title = this.Title.Substring(0,99);
            }
            //assert(!string.IsNullOrWhiteSpace(Source_Id), () => $"'{Source_Id}' is not a valid '{nameof(Transmittal.Source_Id)}' or is null.");
            assert(!string.IsNullOrWhiteSpace(TR_NO), () => $"'{TR_NO}' is not a valid '{nameof(TransmittalSubmitModel.TR_NO)}' or is null.");
            assert(Uri.IsWellFormedUriString(Url, UriKind.Absolute), () => $"'{Url}' is not a valid Url.");
            var validActions = "FirstIssue;ReplyIssue;Forward;RenewIssue";
            //this.TR_ACTION = string.IsNullOrWhiteSpace(this.TR_ACTION) ? "FirstIssue" : this.TR_ACTION;
            assert(validActions.Contains(this.TR_ACTION), () => $"'{TR_ACTION}' is not a valid actionn. Valid actions are:'{validActions}'");

            assert(Documents != null && Documents.Length > 0, () => $"'{Documents?.Length}'. Transmittals should contain at least one file.");
            Documents = Documents.Select(x => x.Validate()).ToArray();
            return this;
        }
    }

}
