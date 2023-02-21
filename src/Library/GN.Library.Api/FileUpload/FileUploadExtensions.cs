
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.FileUpload
{
	public static class FileUploadExtensions
	{
		public static IServiceCollection AddFileUpload(this IServiceCollection services, IConfiguration configuration, Action<FileUploadOptions> configure)
		{
			var options = new FileUploadOptions();
			configuration?.Bind("FileUpload", options);
			configure?.Invoke(options);
			options.Validate(configuration);
			services.AddSingleton(options);
			services.AddSingleton<FileUploadService>();
			services.AddSingleton<IFileUpload>(s => s.GetServiceEx<FileUploadService>());
			services.AddSingleton<IFileUploadService>(s => s.GetServiceEx<FileUploadService>());
			return services;
		}
	}
}
