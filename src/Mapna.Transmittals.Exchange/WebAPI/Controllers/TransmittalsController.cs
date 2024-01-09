using Mapna.Transmittals.Exchange.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mapna.Transmittals.Exchange.Internals;
using GN.Library.SharePoint;
using System.IO;
using Mapna.Transmittals.Exchange.Models;

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

            return Ok("Transmittals Exchange Service is Running.");

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

                    reply.Failed = 0;

                    return Ok(reply);
                }
                catch (Exception err)
                {

                    this.logger.LogError(
                        $"An error occured while receiving transmittal: {transmittal}. We will reject it. Err:'{err.Message}'");


                    return Ok(new SubmitTransmittalReply { Failed = 1, Error = err.GetBaseException().Message, TransmittalId = transmittal.TR_NO });
                }

            }

        }

        [HttpGet]
        [Route("File/{id}")]
        public async Task<ActionResult> File([FromRoute] string id)
        {
            var opt = this.serviceProvider.GetService<TransmittalsExchangeOptions>();
            var fact = this.serviceProvider.GetService<IClientContextFactory>();
            try
            {
                var str = Encoding.UTF8.GetString(System.Convert.FromBase64String(id));
                using (var ctx = fact.CreateContext(opt.ConnectionString))
                {
                    var stream = ctx.OpenFileByUrl(str);
                    this.logger.LogInformation(
                            $"Successfully Served File :'{str}'");
                    return File(stream, "application/octet-stream", Path.GetFileName(str));
                }
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to serve file.");
                return BadRequest(
                    $"Error:{err.Message}");
            }
        }
        [HttpPost]
        [Route("SetResult")]
        public async Task<ActionResult> SetResult([FromBody] MapnaTransmittalFeedbackModel model)
        {
            this.logger.LogInformation(
                $"Tryiing to set result of transmittal: {model.TransmittalNumber}. Code:{model.ResponseCode}");
            try
            {
                using (var repo = this.serviceProvider.GetService<ITransmittalRepository>())
                {
                    if (model.GetResponseCode() == 0)
                    {

                        await repo.SetJobStatus(model.TransmittalNumber, SPJobItem.Schema.Statuses.Completed, model.ResponseDesc);
                    }
                    else
                    {
                        await repo.SetJobStatus(model.TransmittalNumber, SPJobItem.Schema.Statuses.Failed, model.ResponseDesc);
                    }
                    await repo.SetTransmittalIssueState(model.TransmittalNumber, SPTransmittalItem.Schema.IssueStates.Accept);
                }
                return Ok(new MapnaTransmittalFeedbackModel { 
                    TransmittalNumber = model.TransmittalNumber,
                    ResponseCode = "0",
                    ResponseDesc = "Success"
                
                });
                
            }
            catch (Exception err)
            {

                return BadRequest(
                    $"{err.GetBaseException().Message}");
            }
            return Ok();
        }
    }
}
