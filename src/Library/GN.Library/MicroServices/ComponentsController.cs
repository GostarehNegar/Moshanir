using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GN;
using Microsoft.Extensions.Logging;

namespace GN.Library.MicroServices
{
	public class JavaScriptResult : ContentResult
	{
		public JavaScriptResult(string script)
		{
			this.Content = script;
			this.ContentType = "application/javascript";
		}
	}
	[Route("[controller]")]
	[ApiController]
	public class ComponentsController : ControllerBase
	{
		const string template =
@"
document.write('<script type={1} src='{2}</script>');
ReactDOM.render(
  React.createElement(Hello, {toWhat: 'World'}, null),
  document.getElementById('root')
);
";
		private readonly MicroServicesConfiguration configuration;
		private readonly ILogger logger;

		public ComponentsController(MicroServicesConfiguration configuration, ILogger<ComponentsController> logger)
		{
			this.configuration = configuration;
			this.logger = logger;
		}

		// GET api/values
		[HttpGet]
		public ActionResult<IEnumerable<string>> Get()
		{
			return new string[] { "value1", "value2" };
		}

		private bool is_main(string str)
		{
			return str != null && str.EndsWith(".js") && str.StartsWith("main") && str.Count(ch => char.IsDigit(ch)) > 0;
		}
		bool is_chunk(string str)
		{
			return str != null && str.Length > 0 && str.EndsWith(".js") && str.Contains("chunk") && char.IsDigit(str[0]);
		}
		bool is_runtime(string str)
		{
			return str != null && str.Length > 0 && str.EndsWith(".js") && str.StartsWith("runtime") && str.Count(ch => char.IsDigit(ch)) > 0;
		}

		/// <summary>
		///  http://localhost:5000/communications/app/build/logo192.png
		/// </summary>
		/// <param name="area"></param>
		/// <param name="id"></param>
		/// <returns></returns>

		[HttpGet("{area}/{id}")]
		public ActionResult Get(string area, string id)
		{
			ActionResult result = NotFound();
			var micro_service = this.configuration.GetMicroServices()
				.Select(x => x.Validate())
				.FirstOrDefault(x => string.Compare(x.Area, area, true) == 0);
			try
			{
				if (micro_service == null)
				{
					throw new Exception(string.Format(
						"Area: {0} Not Found. We didnot found any micro-service that supports this area.", area));
				}
				var component_name = id;
				var manifest_resource_names = micro_service.ComponentsAssembly.GetManifestResourceNames();
				var main = manifest_resource_names.FirstOrDefault(x => is_main(Path.GetFileName(x?.ToLowerInvariant())));
				var chunk = manifest_resource_names.FirstOrDefault(x => is_chunk(Path.GetFileName(x?.ToLowerInvariant())));
				var run_time = manifest_resource_names.FirstOrDefault(x => is_runtime(Path.GetFileName(x?.ToLowerInvariant())));
				/// We will get the 'main.js', chunk.js' and 'runtime.js files from
				/// the resource assembly. This will be used to build the component html
				/// just like its is included in react index.html file.
				if (main == null)
					throw new Exception(string.Format(
						"'main.js' not found. We failed to find 'main.[hash].chunk.js' in the embeded resources of the compnents assembly:{0}", 
						micro_service.ComponentsAssembly.FullName));
				if (chunk == null)
					throw new Exception(string.Format(
						"'chunk.js' not found. We failed to find '[hash].chunk.js' in the embeded resources of the compnents assembly:{0}",
						micro_service.ComponentsAssembly.FullName));
				if (run_time == null)
					throw new Exception(string.Format(
						"'runtime-main.js' not found. We failed to find 'runtime-mian.[hash].chunk.js' in the embeded resources of the compnents assembly:{0}",
						micro_service.ComponentsAssembly.FullName));
				var baseUrl = string.Format("{0}://{1}", this.HttpContext.Request.Scheme, this.HttpContext.Request.Host.ToUriComponent())
					+ $"/{area}/app/build/static/js/";
				var forwardedAddress = this.Request.GetForwardedHostAddress();
				var apiBase = string.Format("{0}://{1}", this.HttpContext.Request.Scheme, this.HttpContext.Request.Host.ToUriComponent()) + "/api/";
				if (!string.IsNullOrWhiteSpace(forwardedAddress))
				{
					baseUrl = string.Format("{0}/app/build/static/js/", forwardedAddress,area);
					var _urn = new Uri(forwardedAddress);
					apiBase = _urn.AbsoluteUri.Replace(_urn.AbsolutePath, "/api" + _urn.AbsolutePath);

				}
				string js = "";
				var js_template = "document.write('<script type=\"text/javascript\" src=\"{0}\"></script>');";
				js += string.Format("DYNAMIC_BOOT_OPTIONS={{'componentName':'{0}','apiBase':'{1}'}};", id, apiBase);
				js += string.Format(js_template, baseUrl + Path.GetFileName(run_time));
				js += string.Format(js_template, baseUrl + Path.GetFileName(chunk));
				js += string.Format(js_template, baseUrl + Path.GetFileName(main));
				return new JavaScriptResult(js);



			}
			catch (Exception err)
			{
				this.logger.LogError(
					"An error occured while trying to get component. Name:'{0}', Area: '{1}',  Error:{2}", area, id, err.GetBaseException().Message);

			}
			return result;
		}

		// GET api/values/5
		[HttpGet("{id}")]
		public ActionResult Get(string id)
		{
			string js = "";
			//string js_template = "<script type=\"text/javascript\" src=\"{0}\"</script>\r\n";
			var request = this.HttpContext.Request;
			var js_template = "document.write('<script type=\"text/javascript\" src=\"{0}\"></script>');";
			var files = Directory.GetFiles(@".\app\build\static\js", "*.js", SearchOption.AllDirectories).ToList();
			bool is_main(string str)
			{
				return str != null && str.StartsWith("main") && str.Count(ch => char.IsDigit(ch)) > 0;
			}
			bool is_chunk(string str)
			{
				return str != null && str.Length > 0 && str.Contains("chunk") && char.IsDigit(str[0]);
			}
			bool is_runtime(string str)
			{
				return str != null && str.Length > 0 && str.StartsWith("runtime") && str.Count(ch => char.IsDigit(ch)) > 0;
			}

			var main = files.FirstOrDefault(x => is_main(Path.GetFileName(x)));
			var chunk = files.FirstOrDefault(x => is_chunk(Path.GetFileName(x)));
			var runtime = files.FirstOrDefault(x => is_runtime(Path.GetFileName(x)));

			if (main == null)
				throw new Exception("main not found!");
			if (chunk == null)
				throw new Exception("chunk not found!");
			if (runtime == null)
				throw new Exception("runime not found!");

			var baseUrl = string.Format("{0}://{1}", this.HttpContext.Request.Scheme, this.HttpContext.Request.Host.ToUriComponent())
				+ "/static/js/";
			var forwardedAddress = this.Request.GetForwardedHostAddress();
			var apiBase = string.Format("{0}://{1}", this.HttpContext.Request.Scheme, this.HttpContext.Request.Host.ToUriComponent()) + "/api/";
			if (!string.IsNullOrWhiteSpace(forwardedAddress))
			{
				baseUrl = string.Format("{0}/static/js/", forwardedAddress);
				var _urn = new Uri(forwardedAddress);

				apiBase = _urn.AbsoluteUri.Replace(_urn.AbsolutePath, "/api" + _urn.AbsolutePath);

			}
			if (1 == 0)
			{
				js += string.Format(js_template, baseUrl + "bundle.js");
				js += string.Format(js_template, baseUrl + "0.chunk.js");
				js += string.Format(js_template, baseUrl + "main.chunk.js");
			}
			else
			{
				//js += string.Format("comp_name='{0}';", id);
				js += string.Format("DYNAMIC_BOOT_OPTIONS={{'componentName':'{0}','apiBase':'{1}'}};", id, apiBase);
				js += string.Format(js_template, baseUrl + Path.GetFileName(runtime));
				js += string.Format(js_template, baseUrl + Path.GetFileName(chunk));
				js += string.Format(js_template, baseUrl + Path.GetFileName(main));
			}
			return new JavaScriptResult(js);
		}

		// POST api/values
		[HttpPost]
		public void Post([FromBody] string value)
		{
		}

		// PUT api/values/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}

}
