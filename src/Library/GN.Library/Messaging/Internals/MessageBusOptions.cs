using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace GN.Library.Messaging.Internals
{
	public class MessageBusOptions
	{
		public MessageBusOptions()
		{
			//this.ServiceName = AppHost.Utils.GetCurrentApplicationName();
		}
		//public string Endpoint { get; protected set; }
		public string Name { get; set; }
		public bool AddStreamingServices { get; set; }
		public int DefaultTimeout { get; set; } = 5;

		public int NumberOfQueues { get; set; } = 0;

		public int HeartBit { get; set; } = 15;
		//public bool SkipInternal { get; set; }
		public MessageBusOptions UseEndpointNames(string name = null)
		{
			this.Name = name;
			return this.Validate();
		}

		public string GetEndpointName()
		{
			this.Validate();
			var hash = GetHashString(Process.GetCurrentProcess().MainModule.FileName);
			return  $"{this.Name}x{Environment.MachineName}x{hash}";

		}
		public static byte[] GetHash(string inputString)
		{
			using (HashAlgorithm algorithm = SHA1.Create())
				return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
		}

		public static string GetHashString(string inputString)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in GetHash(inputString))
				sb.Append(b.ToString("X2"));

			return sb.ToString();
		}
		public MessageBusOptions Validate()
		{
			if (string.IsNullOrWhiteSpace(this.Name))
			{
				this.Name = string.IsNullOrWhiteSpace(AppInfo.Current.Name)
					? AppHost.Utils.GetCurrentApplicationName()
					: AppInfo.Current.Name;
			}
			this.HeartBit = this.HeartBit < 5 ? 5 : this.HeartBit;

			//if (string.IsNullOrWhiteSpace(this.Endpoint))
			//{
			//	var hash = GetHashString(Process.GetCurrentProcess().MainModule.FileName);
			//	this.Endpoint = $"{this.ServiceName}x{Environment.MachineName}x{hash}";
			//}
			return this;
		}
	}
}
