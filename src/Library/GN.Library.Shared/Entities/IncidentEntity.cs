using GN.Library.Shared.Chats;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Entities
{
    public class IncidentEntity : DynamicEntity
    {
        public new class Schema : DynamicEntity.Schema
        {
            public const string LogicalName = "incident";
        }
    }
}
