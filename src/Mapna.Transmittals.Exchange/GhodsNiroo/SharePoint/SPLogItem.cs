using GN.Library.SharePoint.Internals;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mapna.Transmittals.Exchange.GhodsNiroo.SharePoint
{
    public class SPLogItem : SPItem
    {
        public new class Schema : SPItem.Schema
        {
            public const string Message = "Message";
            public const string Level = "Level";
            //public const string Scope = "Scope";
            public class Levels
            {
                public const string Info = "Info";
                public const string Warn = "Warn";
                public const string Error = "Error";
                public const string Critical = "Critical";
            }
        }
        [Column(Schema.Message)]
        public string Message { get => GetAttibuteValue<string>(Schema.Message); set => SetAttributeValue(Schema.Message, value); }

        //[Column(Schema.Scope)]
        //public string Scope { get => GetAttibuteValue<string>(Schema.Scope); set => SetAttributeValue(Schema.Scope, value); }

        [Column(Schema.Level)]
        public string Level { get => GetAttibuteValue<string>(Schema.Level); set => SetAttributeValue(Schema.Level, value); }
        public SPLogItem SetLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Warning:
                    Level = Schema.Levels.Warn;
                    break;
                case LogLevel.Critical:
                    Level = Schema.Levels.Critical;
                    break;
                case LogLevel.Error:
                    Level = Schema.Levels.Error;
                    break;

                default:
                    Level = Schema.Levels.Info;
                    break;
            }
            return this;
        }
    }
}
