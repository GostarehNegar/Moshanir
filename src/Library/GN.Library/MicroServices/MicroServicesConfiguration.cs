using System;
using System.Collections.Generic;
using System.Text;
using GN.Library.Contracts.Gateway;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Reflection;

namespace GN.Library.MicroServices
{
	public interface IMicorServiceConfiguration
	{
		//Action<ApiPingResultModel> ConfigurePingResult { get; set; }
		Action<MicroServiceModel> Configure { get; set; }
		string Name { get; set; }
		string PathPatterns { get; set; }
		string RemoveFormPath { get; set; }
		string Path { get; set; }
		string Area { get; set; }
		Assembly ComponentsAssembly { get; set; }

		IMicorServiceConfiguration Validate();

	}
	public class MicroServiceConfiguration : IMicorServiceConfiguration
	{
		//public Action<ApiPingResultModel> ConfigurePingResult { get; set; }
		public Action<MicroServiceModel> Configure { get; set; }
		public string Name { get; set; }
		public string PathPatterns { get; set; }
		public string RemoveFormPath { get; set; }
		public string Path { get; set; }
		public Assembly ComponentsAssembly { get; set; }
		public string Area { get; set; }
		public MicroServiceConfiguration()
		{
			this.Configure = cfg => {
				cfg.Name = this.Name;
				cfg.PathPatterns = this.PathPatterns;
				cfg.RemoveFormPath = this.RemoveFormPath;
			};
		}

		public IMicorServiceConfiguration Validate()
		{

			return this;
			
		}



	}
	public class MicroServicesConfiguration 
	{
		public static MicroServicesConfiguration Current = new MicroServicesConfiguration();
		private List<IMicorServiceConfiguration> configurations = new List<IMicorServiceConfiguration>();
		public Action<ApiPingResultModel> ConfigurePingResult { get; set; }
		public string GatewayServerUrl { get; set; }
		public int Puase { get; set; }

		

		public MicroServicesConfiguration()
		{
			this.Puase = 100;
		}
		public MicroServicesConfiguration Validate(IConfiguration configuration)
		{
			this.Puase = this.Puase < 10 ? 10 : this.Puase;
			return this;
		}
		public MicroServicesConfiguration AddMicroService(IMicorServiceConfiguration configuration)
		{
			this.configurations.Add(configuration);
			return this;
		}
		public MicroServicesConfiguration AddMicroService(Action<MicroServiceConfiguration> configure)
		{
			var config = new MicroServiceConfiguration();
			configure?.Invoke(config);
			return this.AddMicroService(config);
		}

		public IEnumerable<IMicorServiceConfiguration> GetMicroServices()
		{
			return this.configurations;
		}

		public string GetUrl()
		{
			string result = string.Empty;
			AppHost.Utils.GetAppUrls()
				.ToList()
				.ForEach(x =>
				{
					result += (string.IsNullOrWhiteSpace(result) ? "" : ";") + x;
				});
			return result;
		}

	}
}
