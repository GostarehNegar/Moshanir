
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

namespace GN.Library.Controllers
{
	[Route("api/[controller]")]
	public class XrmApi2Controller : Controller
	{
		protected static readonly ILogger log = typeof(XrmApi2Controller).GetLoggerEx();
		private IWebCommandFactory webCommandFactory;

		public XrmApi2Controller(IWebCommandFactory factory)
		{
			this.webCommandFactory = factory;
		}
		[HttpGet]
		//[Route("test")]
		public string Get()
		{
			return "Message from XrmAPI. I am OK!.";
		}


		[HttpPost]
		public ActionResult Post([FromBody] WebCommandRequest request)
		{
			//WebCommandRequest request = null;
			var user = HttpContext.User;
			//log.MethodStart();
			log.LogInformation("XrmJS Service API starts. Request: {0}", request);
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
				log.LogTrace("Service successfully executed.", ret);
				//if (!string.IsNullOrWhiteSpace(ret.Redirect))
				//{
				//	//var response = Request.CreateResponse(HttpStatusCode.Moved);
				//	//response.Headers.Location = new Uri("http://www.abcmvc.com");
				//	return this.RedirectPermanent("http://www.google.com");
				//};

			}
			catch (Exception e)
			{
				if (e.Message== "Unauthorized")
				{
					return Unauthorized();
				}

				//if (log.HandleException(e))
				//{
				//	//throw;
				//}
				ret.Status = CommandStatus.Error;
				ret.Message = e.Message;
			}
			success = ret.Status == CommandStatus.Success;// ? CommandStatus.Success : CommandStatus.Failed;
														  //log.MethodReturns(success);

			var result = new OkObjectResult(ret);
			return result;


			//			return Request.CreateResponse(HttpStatusCode.OK, ret);
		}


		//public string GetHtmlResource(string resourceName)
		//{
		//    log.MethodStart();
		//    string urlAddress = resourceName;
		//    var data = "";
		//    log.InfoFormat(
		//        "Starts: {0}", resourceName);
		//    try
		//    {
		//        if (resourceName.ToLower().StartsWith("http://crm:6139/"))
		//        {
		//            resourceName = resourceName.ToLower().Replace("http://crm:6139/", "http://localhost:6139/");
		//            log.InfoFormat("Changed to : {0}", resourceName);

		//        }
		//        resourceName = resourceName.ToLower().Replace("http://tpm.gnco.ir:2350/", "http://localhost:2350/");
		//        resourceName = resourceName.ToLower().Replace("http://mwt.gnco.ir:2350/", "http://localhost:2350/");
		//        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resourceName);
		//        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
		//        request.UserAgent =
		//            "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; WOW64; " +
		//            "Trident/4.0; SLCC1; .NET CLR 2.0.50727; Media Center PC 5.0; " +
		//            ".NET CLR 3.5.21022; .NET CLR 3.5.30729; .NET CLR 3.0.30618; " +
		//            "InfoPath.2; OfficeLiveConnector.1.3; OfficeLivePatch.0.0)";

		//        if (response.StatusCode == HttpStatusCode.OK)
		//        {
		//            Stream receiveStream = response.GetResponseStream();
		//            StreamReader readStream = null;

		//            if (response.CharacterSet == null)
		//            {
		//                readStream = new StreamReader(receiveStream);
		//            }
		//            else
		//            {
		//                readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
		//            }

		//            data = readStream.ReadToEnd();

		//            response.Close();
		//            readStream.Close();
		//        }
		//    }
		//    catch (Exception err)
		//    {
		//        log.ErrorFormat("An error occored:{0}", err.Message);
		//    }
		//    bool success = true;
		//    log.InfoFormat(
		//        "Data Length: {0}", data.Length);

		//    return data;
		//    var root = Path.Combine(System.Web.HttpContext.Current.Server.MapPath(@"~/root/"), "");
		//    return System.Web.HttpContext.Current.Server.MapPath(@"~");
		//    if (root.ToLower().Contains("ipa.web"))
		//    {
		//        root = root.Substring(0, root.ToLower().IndexOf("ipa.web")) + "IPA.CrmResources\\app\\Views";
		//        log.DebugFormat("ROOT:{0}", root);

		//    }
		//    var fileName = Path.Combine(root, resourceName);
		//    log.DebugFormat(
		//        "Trying to load resource from:'{0}'", fileName);
		//    var txt = string.Format("Error loading resource. FileName:{0}", fileName);
		//    try
		//    {
		//        txt = System.IO.File.ReadAllText(fileName);
		//    }
		//    catch (Exception e)
		//    {
		//        if (log.HandleException(e))
		//        {

		//        }
		//        log.ErrorFormat("Failed to load resource at {0}", root + resourceName);
		//        success = false;


		//    }
		//    log.InfoFormat(
		//        "Starts: {0}", resourceName);

		//    log.MethodReturns(success);


		//    return txt;

		//}


		////public string PlugingEvent(string message, string entityName, Guid Id)
		////{

		////	return "";
		////}
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