using GN.Library.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.EntityServices
{
    public class DynamicEntityUpdatedEx
    {
        public string LogialName { get; set; }
        public string Id { get; set; }

        public DynamicEntity PreImage { get; set; }
        public DynamicEntity PostImage { get; set; }
        public DynamicEntity Entity { get; set; }
        public string MessageName { get; set; }
    }
}
