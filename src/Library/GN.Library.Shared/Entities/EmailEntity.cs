using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Entities
{
    public class EmailEntity : DynamicEntity
    {
        public new class Schema : DynamicEntity.Schema
        {
            public const string LogicalName = "email";
        }
    }
}
