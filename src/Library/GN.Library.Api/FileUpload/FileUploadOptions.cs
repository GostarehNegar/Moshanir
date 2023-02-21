using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GN.Library.FileUpload
{
	public class FileUploadOptions
	{
		public string Folder { get; set; }

		public bool Validate(IConfiguration configuration)
		{
			if (string.IsNullOrWhiteSpace(Folder))
			{
				Folder = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "FileUpload"));
			}
			else
			{
				Folder = Path.GetFullPath(Folder);
			}
			if (!Directory.Exists(Folder))
			{
				Directory.CreateDirectory(Folder);
			}

			return Directory.Exists(Folder);
		}
	}
}
