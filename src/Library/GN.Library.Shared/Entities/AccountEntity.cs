using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Entities
{
    public class AccountEntity : XrmDynamicEntity
    {
        public new class Schema : DynamicEntity.Schema
        {
            public const string LogicalName = "account";
        }
        public AccountEntity()
        {
            this.LogicalName = Schema.LogicalName;
        }
    }
}
