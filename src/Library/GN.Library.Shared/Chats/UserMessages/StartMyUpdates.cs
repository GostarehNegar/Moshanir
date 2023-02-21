using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Chats.UserMessages
{
    public class StartMyUpdates
    {
        public long LastUpdate { get; set; }
        public string UserId { get; set; }
    }

    public class GetEntityUpdate
    {
        public DynamicEntity Entity { get; set; }
    }
    public class MyUpdate
    {
        public string Mode { get; set; }
        public long Until { get; set; }
        public PackedDynamicEntityReference[] PackedActivities { get; set; }
        public DynamicEntity[] Posts { get; set; }
        public PackedDynamicEntityReference [] PackedEntities { get; set; }

        public DynamicEntity[] Activities { get; set; }
        public DynamicEntity[] Entities { get; set; }
        public int Count()
        {
            return (PackedActivities?.Length ?? 0) + (Posts?.Length ?? 0) + (PackedEntities?.Length ?? 0);
        }
    }
}
