using Mapna.Transmittals.Exchange.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Mapna.Transmittals.Exchange.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransmittalsController : Controller
    {
        private readonly ILogger<TransmittalsController> logger;
        private readonly IServiceProvider serviceProvider;

        public TransmittalsController(ILogger<TransmittalsController> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }
        [HttpGet]
        [Route("ping")]
        public async Task<ActionResult<string>> Ping()
        {

            return Ok("ok");

        }
        [HttpPost]
        public async Task<ActionResult<SubmitTransmittalReply>> Submit([FromBody] TransmittalSubmitModel transmittal)
        {
            using (var scope = this.serviceProvider.CreateScope())
            {
                try
                {
                    this.logger.LogInformation(
                        $"Controller recieved a new transmittal: {transmittal}. We will try to enqueue it.");
                    var service = scope.ServiceProvider.GetService<ITransmittalService>();
                    var reply = await service.Submit(transmittal);
                    reply.TransmittalId = transmittal.TR_NO;
                    this.logger.LogInformation(
                        $"Transmittal: {transmittal} successfully queued. {reply.TransmittalId}");

                    reply.Failed = false;

                    return Ok(reply);
                }
                catch (Exception err)
                {
                    
                    this.logger.LogError(
                        $"An error occured while receiving transmittal: {transmittal}. We will reject it. Err:'{err.Message}'");


                    return Ok(new SubmitTransmittalReply { Failed = true, Error = err.GetBaseException().Message, TransmittalId= transmittal.TR_NO });
                }

            }

        }
        [HttpGet]
        [Route("test")]
        public async Task<ActionResult> Test()
        {
            return Ok("ddd");
        }
    }
}
