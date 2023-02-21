
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
using GN.Library.Security;

namespace GN.Library.Controllers
{
	[Route("api/[controller]")]
	public class WebCommandController : Controller
	{
		protected static readonly ILogger_Deprecated log = typeof(WebCommandController).GetLogger();
		private IWebCommandFactory webCommandFactory;

		public WebCommandController(IWebCommandFactory factory)
		{
			this.webCommandFactory = factory;
		}
		[HttpGet]
		[Route("ping")]
		public string Get()
		{
			return "WebCommandController: I am OK!.";
		}
		[HttpPost]
		public ObjectResult Post([FromBody] WebCommandRequest request)
		{

			//log.MethodStart();
			log.InfoFormat("WebCommandController starts. Request: {0}", request);
			bool success = false;
			WebCommandResponse ret = new WebCommandResponse();
			try
			{
				//GNServiceLocator.Extensions.ApplicationServices();
				if (request == null)
					throw new ArgumentException(string.Format(
						"Invalid or NULL request."));
				var service = this.webCommandFactory.Create(request.Request);// GlobalContext.Current.InfarstructureServices.Resolver.GetService<IWebCommand>(request.Request);
				if (service == null)
					throw new ArgumentException(
						string.Format("Service Not Found. Request:{0}", request));
				log.LogTrace("Servie successfully resolved. Service: {0}", service);
				ret = service.Handle(request);
				//if (!string.IsNullOrWhiteSpace(ret.Redirect))
				//{
				//	this.RedirectPermanent(ret.Redirect);
				//};
				log.LogTrace("Service successfully executed.", ret);
			}
			catch (Exception e)
			{
				if (log.HandleException(e))
				{
					//throw;
				}
				ret.Status = CommandStatus.Error;
				ret.Message = e.Message;
			}
			success = ret.Status == CommandStatus.Success;
			var result = new OkObjectResult(ret);
			return result;
		}
		
		//public HttpResponseMessage GetUpdateVoice(Guid id)
		//{
		//    HttpResponseMessage result = null;
		//    log.MethodStart("id:{0}", id);
		//    bool success = false;
		//    try
		//    {
		//        result = new HttpResponseMessage(HttpStatusCode.OK);
		//        using (var ctx = ServiceLocator.Extensions.TelegramSoultion().ServiceContext)
		//        {
		//            var dataContext = ctx.DataContext;
		//            var update = dataContext.ChatUpdates.Get(id);
		//            var updateData =update.GetContext().GetAnnotaionServices().GetObject<GN.Telegram.Chat.ChatUpdate>(null, null);
		//            var stream = new MemoryStream(updateData.Audio.Content);
		//            result.Content =new StreamContent(stream);
		//            result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/ogg");
		//        }


		//        success = true;
		//    }
		//    catch (Exception e)
		//    {
		//        if (log.HandleException(e))
		//        {

		//        }
		//        result = null;

		//    }
		//    log.MethodReturns(success);
		//    return result;


		//}

	}
}