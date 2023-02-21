using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GN.Library.Contracts.Gateway;
using Microsoft.Extensions.Logging;

namespace GN.Library.MicroServices
{
	[Route("api/[controller]")]
	[ApiController]
	public class PingController : ControllerBase
	{
		private readonly ILogger logger;
		private readonly MicroServicesConfiguration options;
		public PingController(MicroServicesConfiguration options, ILogger<PingController> logger)
		{
			this.options = options;
			this.logger = logger;
		}

		// GET api/values
		[HttpGet]
		public ActionResult<ApiPingResultModel> Get()
		{
			var result = new ApiPingResultModel();
			var items = new List<MicroServiceModel>();
			this.options.ConfigurePingResult?.Invoke(result);
			var microServices = this.options.GetMicroServices();
			foreach (var micro in this.options.GetMicroServices())
			{
				var item = new MicroServiceModel();
				micro.Configure?.Invoke(item);
				items.Add(item);
			}
			result.MicroServices = items.ToArray();
			this.logger.LogTrace(
				"Ping Request Successfully Processed. Result:{0}", result);

			return result;
		}

	}


}
