
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Text;
using GN.Library.WebCommands;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using Microsoft.AspNetCore.Http;
using GN.Library.Services;

namespace GN.Library.Controllers
{
	[Route("[controller]")]
	public class FileController : Controller
	{
		private readonly ILogger logger;
		private readonly IFileStorage storage;
		public FileController(ILogger<FileController> logger, IFileStorage storage)
		{
			this.logger = logger;
			this.storage = storage;
		}
		[HttpGet]
		[Route("{id}")]
		public ActionResult GetFile([FromRoute] string id)
		{
			ActionResult result = NotFound();
			if (Guid.TryParse(id, out var _id))
			{
				var file = storage.Get(_id);
				if (file!=null)
				{
					result = File(file.Contents, file.ContentType);
				}
			}
			return result;
		}

	}
}