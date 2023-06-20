using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GN.Library.Shared.Chats.UserMessages
{
    public class StartMyUpdates
    {
        public long LastSynchedOn { get; set; }
        public string UserId { get; set; }
    }

    public class GetEntityUpdate
    {
        public DynamicEntity Entity { get; set; }
    }
    public class MyUpdate
    {
        public string Type { get; set; }
        public DynamicEntity Target { get; set; }
        public string Mode { get; set; }
        public long LastSynchedOn { get; set; }
        public string Description { get; set; }
        public PackedDynamicEntityReference[] PackedActivities { get; set; }
        public DynamicEntity[] Posts { get; set; }
        public PackedDynamicEntityReference [] PackedEntities { get; set; }

        public DynamicEntity[] Activities { get; set; }
        public DynamicEntity[] Entities { get; set; }
        public int Count()
        {
            return (PackedActivities?.Length ?? 0) + (Posts?.Length ?? 0) + (PackedEntities?.Length ?? 0) + (Entities?.Length ?? 0)
                + (Activities?.Length ?? 0);
        }
        public MyUpdate AddPackedEntities(PackedDynamicEntityReference[] items)
        {
            this.PackedEntities = this.PackedEntities ?? new PackedDynamicEntityReference[] { };
            this.PackedEntities = this.PackedEntities.Concat(items).ToArray();
            return this;
        }
        public MyUpdate AddPackedActivities(PackedDynamicEntityReference[] items)
        {
            this.PackedActivities = this.PackedActivities ?? new PackedDynamicEntityReference[] { };
            this.PackedActivities = this.PackedActivities.Concat(items).ToArray();
            return this;
        }
        public MyUpdate Clear()
        {
            this.PackedActivities = new PackedDynamicEntityReference[] { };
            this.PackedEntities = new PackedDynamicEntityReference[] { };
            this.Activities = new DynamicEntity[] { };
            this.Entities = new DynamicEntity[] { };
            return this;
        }
    }
}
