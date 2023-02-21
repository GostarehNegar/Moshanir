using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.StaticResourceFiles
{
	public class StaticResourceFileOptions
	{
		public static StaticResourceFileOptions Current = new StaticResourceFileOptions();
		private List<StaticResourceFileProfile> profiles = new List<StaticResourceFileProfile>();

		public StaticResourceFileOptions AddProfile(Action<StaticResourceFileProfile> configure)
		{
			return this;
		}

	}
}
