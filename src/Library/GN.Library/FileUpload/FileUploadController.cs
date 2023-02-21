using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.FileUpload
{
	[Route("[controller]")]
	public class UploadController : Controller
	{
		private readonly FileUploadOptions options;
		public UploadController(FileUploadOptions options)
		{
			this.options = options;

		}
		[HttpPut("{server}/{jid}/{secret}/{name}")]
		public async Task<ActionResult> Put([FromRoute] string server, [FromRoute] string jid, [FromRoute] string secret, [FromRoute]string name)
		{
			try
			{
				var fileName = Path.Combine(this.options.Folder, server, secret, name);
				if (!Directory.Exists(Path.GetDirectoryName(fileName)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(fileName));
				}
				using (FileStream DestinationStream = System.IO.File.Create(fileName))
				{
					await Request.Body.CopyToAsync(DestinationStream);
				}
				return Ok();
			}
			catch (Exception)
			{

			}
			return BadRequest();

		}
		[HttpGet("{server}/{jid}/{secret}/{name}")]
		public async Task<ActionResult> Get([FromRoute] string server, [FromRoute] string jid, [FromRoute] string secret, [FromRoute]string name)
		{
			try
			{
				var fileName = Path.Combine(this.options.Folder, server, secret, name);
				string mime = string.Empty;

				switch(Path.GetExtension(fileName)?.ToLowerInvariant())
				{
					case "jpg":
						mime = "application/jpeg";
						break;
					default:
						mime = "application/pdf";
						break;
				}

				return File(System.IO.File.ReadAllBytes(fileName), mime);
			}
			catch (Exception err)
			{

			}
			return NotFound();

		}

	}
}
