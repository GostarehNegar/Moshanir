using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library
{
	public interface ICurrentUser
	{
		IDictionary<string,object> Properties { get; }
		Guid Id { get; set; }
		string Token { get; set; }
		ICurrentUser Clone();
	}

	public class CurrentUser : ICurrentUser
	{
		private ConcurrentDictionary<string, object> _properties = new ConcurrentDictionary<string, object>();
		public bool Inited { get; set; }
		public Guid Id { get; set; }
		public string Token { get; set; }

		public void Initialize(Guid id, string token, IDictionary<string,object> values)
		{
			this.Inited = true;
			this.Id = id;
			this.Token = token;
			this._properties = new ConcurrentDictionary<string, object>(values);

		}

		public IDictionary<string, object> Properties => _properties;

		public ICurrentUser Clone()
		{
			return new CurrentUser
			{
				Id = this.Id,
				Token = this.Token,
				_properties = new ConcurrentDictionary<string, object>(this._properties)
			};
		}
	}
}
