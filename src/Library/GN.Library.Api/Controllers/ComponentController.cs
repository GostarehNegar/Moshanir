
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

namespace GN.Library.Controllers
{
	class LoadComponentModel
	{
		public string Component { get; set; }
	}
	[Route("[controller]")]
	public class ComponentsController : Controller
	{
		private readonly ILogger logger;
		public ComponentsController(ILogger<ComponentsController> logger)
		{
			this.logger = logger;
		}
		[HttpGet]
		[Route("LoadInWebResource")]
		public ActionResult LoadInWebResource([FromQuery] string location)
		{
			ActionResult result;
			string ComponentName = null;
			var str = HttpUtility.UrlDecode(location)?.ToLowerInvariant();
			if (!string.IsNullOrWhiteSpace(str))
			{
				try
				{
					var dataStart = str.IndexOf("?data=");
					if (dataStart > -1)
					{
						var dataEnd = str.IndexOf('}', dataStart);
						if (dataEnd > dataStart)
						{
							dataStart = dataStart + "?data=".Length;
							var dataStr = str.Substring(dataStart, dataEnd + 1 - dataStart);
							var model = Newtonsoft.Json.JsonConvert.DeserializeObject<LoadComponentModel>(dataStr);
							ComponentName = model?.Component;
						}
					}
				}
				catch { }
			}
			var baseUrl = string.Format("{0}://{1}", this.HttpContext.Request.Scheme, this.HttpContext.Request.Host.ToUriComponent()) +
				"/components";
			result = string.IsNullOrWhiteSpace(ComponentName)
				? (ActionResult)new BadRequestResult()
				: this.Redirect($"{baseUrl}/{ComponentName}");
			return result;
		}

	}
}