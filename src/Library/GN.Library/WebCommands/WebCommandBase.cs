using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GN;

namespace GN.Library.WebCommands
{



	public abstract class WebCommandBase<T1, T2> : IWebCommand
		where T1 : class, new()
		where T2 : class, new()
	{
		//protected static readonly ILogger log = AppHost.Services.GetService. (typeof(WebCommandBase<T1, T2>));
		protected WebCommandRequest baseRequest;
		protected T2 _reply;
		protected string name;
		public WebCommandBase()
		{
			//dataContext = GNServiceLocator.GetService<IPA.Xrm.IXrmDataContextEx>();


		}

		protected T1 Deserialize(string str)
		{
			try
			{
				return AppHost.Utils.Deserialize<T1>(str);
			}
			catch { }
			return default(T1);
		}

		protected string Serialize(T2 data)
		{
			//return Newtonsoft.Json.JsonConvert.SerializeObject(data);
			return AppHost.Utils.Serialize2(data);
		}
		protected WebCommandResponse response;

		public WebCommandResponse Handle(WebCommandRequest request)
		{
			this.baseRequest = request;
			//            dataContext = dataContext.Clone();
			var req = Deserialize(request.Data);
			var ret = new WebCommandResponse();
			this.response = ret;
			this._reply = new T2();
			ret.Status = DoHandle(req, this._reply);
			ret.Data = Serialize(this._reply);
			return ret;
		}
		protected abstract CommandStatus DoHandle(T1 request, T2 reply);
		public virtual string GetName()
		{
			return this.name;
		}
		public string Name { get { return GetName(); } }
		public override string ToString()
		{
			return string.Format("{0}", Name);
		}
		protected Guid? FixUknwnUserGuid(Guid? id)
		{
			try
			{
				return id.HasValue && id.ToString() == "00000000-0000-0000-0000-000000000001"
					? Guid.Empty// DataContext.XrmSystemUsers.GetSystemAdministrator().Id
					: id;
			}
			catch
			{

			}
			return id;
		}

		protected Guid? GetCurrentUserId()
		{
			return this.baseRequest == null
				? (Guid?)null
				: FixUknwnUserGuid(this.baseRequest.CurrentUserId);
		}
		protected string GetDeviceId()
		{
			return this.baseRequest.DeviceId;
		}
	}


}
