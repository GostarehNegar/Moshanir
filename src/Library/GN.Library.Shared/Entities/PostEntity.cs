using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Entities
{
    public class PostEntity : DynamicEntity
    {
        public new class Schema : DynamicEntity.Schema {
            public const string LogicalName = "post";
            public const string PostId = LogicalName + "id";
            public const string Text = "text";
            public const string RegardingObjectId = "regardingobjectid";
            public const string Post_Comments = "Post_Comments";
        }
        public override long Time { get => (this.GetAttributeValue<DateTime?>(Schema.CreatedOn) ?? DateTime.UtcNow).Ticks; }

        public string Text => this.GetAttributeValue<string>("text");

        public DynamicEntityReference Regarding
        {
            get => this.GetAttributeValue<DynamicEntityReference>(Schema.RegardingObjectId);
            set => this.SetAttributeValue(Schema.RegardingObjectId, value);
        }

        public PostEntity Validate()
        {
            this.Attributes = this.Attributes ?? new DynamicAttributeCollection();
            this.RelatedObjects = this.RelatedObjects ?? new DynamicEntityCollection();
            return this;
        }
    }
}
