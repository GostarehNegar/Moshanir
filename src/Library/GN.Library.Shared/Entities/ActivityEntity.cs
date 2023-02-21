using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Entities
{
    public class ActivityEntity : DynamicEntity
    {
        public new class Schema : DynamicEntity.Schema
        {
            public const string Subject = "subject";
        }
        public string Subject
        {
            get => this.GetAttributeValue<string>(Schema.Subject);
            set => this.SetAttributeValue(Schema.Subject, value);
        }

        public ActivityEntity Validate()
        {
            return this;
        }
    }

}
