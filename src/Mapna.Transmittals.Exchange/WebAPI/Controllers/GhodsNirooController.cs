using Mapna.Transmittals.Exchange.GhodsNiroo;
using Mapna.Transmittals.Exchange.GhodsNiroo.Incoming;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapna.Transmittals.Exchange.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GhodsNirooController : Controller
    {
        public class SubmitReply
        {
            public int Failed { get; set; }
            public string Error { get; set; }
            public string TransmittalId { get; set; }
        }
        private readonly IGhodsNirooIncomingQueue queue;
        private readonly ILogger<GhodsNirooController> logger;

        public GhodsNirooController(IGhodsNirooIncomingQueue queue, ILogger<GhodsNirooController> logger)
        {
            this.queue = queue;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<SubmitReply>> Submit([FromBody] IncomingTransmittalRequest transmittal)
        {
            await Task.CompletedTask;
            var result = new SubmitReply();

            try
            {
                this.logger.LogInformation(
                    $"GhodsNiroo Controller Received a Transmittal. We will try to enqueue it. {transmittal}");
                queue.Enqueue(transmittal.Validate());
                return Ok(new SubmitReply { Failed = 0, Error = err.GetBaseException().Message, TransmittalId = transmittal.TR_NO });
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to enqueue transmittal. Err:{err.GetBaseException().Message}");
                return Ok(new SubmitReply { Failed = 1, Error = err.GetBaseException().Message, TransmittalId = transmittal.TR_NO });
            }


        }
    }
}
