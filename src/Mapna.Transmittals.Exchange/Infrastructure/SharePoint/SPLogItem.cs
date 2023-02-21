using GN.Library.SharePoint.Internals;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mapna.Transmittals.Exchange.Internals
{
    public class SPLogItem : SPItem
    {
        public new class Schema : SPItem.Schema
        {
            public const string Message = "Message";
            public const string Level = "Level";
            public class Levels
            {
                public const string Info = "Info";
                public const string Warn = "Warn";
                public const string Error = "Error";
                public const string Critical = "Critical";
            }
        }
        [Column(Schema.Message)]
        public string Message { get => this.GetAttibuteValue<string>(Schema.Message); set => this.SetAttributeValue(Schema.Message, value); }

        [Column(Schema.Level)]
        public string Level { get => this.GetAttibuteValue<string>(Schema.Level); set => this.SetAttributeValue(Schema.Level, value); }
        public SPLogItem SetLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Warning:
                    this.Level = Schema.Levels.Warn;
                    break;
                case LogLevel.Critical:
                    this.Level = Schema.Levels.Critical;
                    break;
                case LogLevel.Error:
                    this.Level = Schema.Levels.Error;
                    break;

                default:
                    this.Level = Schema.Levels.Info;
                    break;
            }
            return this;
        }
    }
}
