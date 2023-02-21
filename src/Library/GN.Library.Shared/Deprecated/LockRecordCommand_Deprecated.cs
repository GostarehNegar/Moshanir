using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Deprecated
{
	public class LockRecordCommand_Deprecated
	{
		public string Key { get; set; }
		public DateTime? ExpiresOn { get; set; }
	}

	public class LockRecordCommandResult_Deprecated
	{
		public string Key { get; set; }

	}
}
