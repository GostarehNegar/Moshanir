using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.EntityServices
{
    public class UpsertEntityCommand
    {
        public DynamicEntity Entity { get; set; }
    }

    public class UpsertEntityRespond
    {
        public DynamicEntity Entity { get; set; }
    }
    public class DeleteEntityCommand
    {
        public DynamicEntity Entity { get; set; }
    }
    public class DeleteEntityResponse
    {
        public DynamicEntity Entity { get; set; }
    }
    public class GetEntityCommand
    {
        public string Id { get; set; }
        public string LogicalName { get; set; }
    }

    public class GetEntityResponse
    {
        public DynamicEntity Entity { get; set; }
    }




}
