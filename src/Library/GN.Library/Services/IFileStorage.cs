using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Services
{
	public class StorageFileModel
	{
		public Guid Id { get; set; }
		public byte[] Contents { get; set; }
		public string ContentType { get; set; }

	}
	public interface IFileStorage
	{
		StorageFileModel Store(StorageFileModel file);
		StorageFileModel Get(Guid id);
	}

	class FileStorage : IFileStorage
	{
		public ConcurrentDictionary<Guid, StorageFileModel> _files = new ConcurrentDictionary<Guid, StorageFileModel>();
		public StorageFileModel Get(Guid id)
		{
			return _files.TryGetValue(id, out var result)
				? result
				: null;
		}

		public StorageFileModel Store(StorageFileModel file)
		{
			if (file.Id == Guid.Empty)
				file.Id = Guid.NewGuid();
			if (string.IsNullOrWhiteSpace(file.ContentType))
				file.ContentType = "application/pdf";
			this._files.AddOrUpdate(file.Id, file, (i, x) => file);
			return file;

		}
	}
}
