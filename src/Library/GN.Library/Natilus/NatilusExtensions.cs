using GN.Library.Natilus.Internals;
using GN.Library.Natilus.Messaging;
using GN.Library.Natilus.Messaging.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Natilus
{
    public static class NatilusExtensions
    {
        public static IServiceCollection AddNatilus(this IServiceCollection services, IConfiguration configuration, Action<NatilusOptions> configure = null)
        {
            var options = new NatilusOptions();
            configure?.Invoke(options);

            services.AddSingleton(options);
            services.AddSingleton<NatilusConnectionProvider>();
            services.AddTransient<INatilusConnectionProvider>(s => s.GetServiceEx<NatilusConnectionProvider>());
            services.AddSingleton<NatilusBus>();
            services.AddTransient<INatilusBus>(s => s.GetServiceEx<NatilusBus>());
            //services.AddHostedService(s => s.GetService<NatilusBus>());
            //services.AddHostedService(s => s.GetService<NatilusConnectionProvider>());
            services.AddTransient<IHealthCheck>(s => s.GetServiceEx<NatilusConnectionProvider>());

            return services;
        }
        internal static string ToString(object o)
        {
            return o?.ToString();
        }
        internal static T ParsePrimitive<T>(string st)
        {
            if (string.IsNullOrWhiteSpace(st))
                return default(T);
            if (typeof(T) == typeof(string))
                return (T)(object)st;
            if (typeof(T) == typeof(int) && int.TryParse(st, out var ires))
                return (T) (object)ires;
            if (typeof(T) == typeof(Guid) && Guid.TryParse(st, out var gres))
                return (T)(object)gres;




            return default(T);

        }
    }
}
