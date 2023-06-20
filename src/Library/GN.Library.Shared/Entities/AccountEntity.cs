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
			public const string AccountId = "accountid";
			public const string Name = "name";
			public const string Telephone1 = "telephone1";
			public const string Telephone2 = "telephone1";
			public const string Telephone3 = "telephone1";
			public const string Fax = "fax";
			public const string Address1_Line1 = "address1_line1";
			public const string Address1_Line2 = "address1_line2";
			public const string Address1_Line3 = "address1_line2";
			public const string Address1_Fax = "address1_fax";
			public const string Address1_City = "address1_city";
			public const string Address1_Telehone1 = "address1_telephone1";
			public const string Address1_Telehone2 = "address1_telephone2";
			public const string Address1_Telehone3 = "address1_telephone3";
			public const string WebSiteUrl = "websiteurl";
			public const string AccountNumber = "accountnumber";
		}
        public AccountEntity()
        {
            this.LogicalName = Schema.LogicalName;
        }
    }
}
