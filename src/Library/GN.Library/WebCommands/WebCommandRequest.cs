using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.WebCommands
{
	public class WebCommandRequest
	{
		public string Request { get; set; }
		public string Data { get; set; }
		public Guid? CurrentUserId { get; set; }
		public bool UseAdminAccount { get; set; }
		public string CurrentUserName { get; set; }
		public string CurrentUserKey { get; set; }
		public string DeviceId { get; set; }
		public override string ToString()
		{
			return string.Format("Command:'{0}', CurrentUserId:'{1}', UseAdminAccount:'{2}'", Request, CurrentUserId, UseAdminAccount);
		}
		public T Deserialize<T>()
		{
			throw new NotImplementedException();

		}


	}

}
