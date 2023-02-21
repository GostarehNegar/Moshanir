using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;

namespace GN.Library.Collections
{
	public interface IObjectCollection : IDictionary<string, object>
	{
	}
	public class ObjectCollection : ConcurrentDictionary<string, object>, IObjectCollection
	{

	}
}
