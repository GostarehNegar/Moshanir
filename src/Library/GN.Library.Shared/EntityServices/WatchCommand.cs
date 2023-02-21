using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.EntityServices
{
    public class WatchCommand
    {
        public string[] LogicalNames { get; set; }

        public string [] GetLogicalNamesOrEmpty()
        {
            return this.LogicalNames ?? new string[] { };
        }
    }
}
