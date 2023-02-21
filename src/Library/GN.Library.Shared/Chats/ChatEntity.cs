using System;
using System.Collections.Generic;
using System.Linq;

namespace GN.Library.Contracts.Chats
{

    public class ChatEntityReference
    {
        public string Id { get; set; }
        public string LogicalName { get; set; }
    }
    public class ChatEntity : DynamicEntity
    {
        public class Schema
        {
            public const string ModiefiedOn = "modifiedon";
            //public const string Name = "name";
            public const string Removed = "removed";
            public const string Action = "action";
            public const string Description = "description";

        }
        //public string Name { get => this.GetAttributeValue(Schema.Name); set => this.SetAttributeValue(Schema.Name, value); }
        //public bool Removed { get => this.GetAttributeValue<bool>(Schema.Removed); set => this.SetAttributeValue(Schema.Removed, value); }
        //public string Action { get => this.GetAttributeValue(Schema.Action); set => this.SetAttributeValue(Schema.Action, value); }
        //public string Description { get => this.GetAttributeValue(Schema.Description); set => this.SetAttributeValue(Schema.Description, value); }

        //public DateTime? ModifiedOnEx => this.GetAttributeValue<DateTime?>(Schema.ModiefiedOn);
        

    }


}
