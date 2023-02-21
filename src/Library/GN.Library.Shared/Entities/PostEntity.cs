using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Entities
{
    public class PostEntity : DynamicEntity
    {
        public new class Schema : DynamicEntity.Schema { }
        public override long Time { get => (this.GetAttributeValue<DateTime?>(Schema.CreatedOn) ?? DateTime.UtcNow).Ticks; }

        public string Text => this.GetAttributeValue<string>("text");

        public PostEntity Validate()
        {
            this.Attributes = this.Attributes ?? new DynamicAttributeCollection();
            this.RelatedObjects = this.RelatedObjects ?? new DynamicEntityCollection();
            return this;
        }
    }
}
