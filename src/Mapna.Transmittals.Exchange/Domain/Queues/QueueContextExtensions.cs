using Mapna.Transmittals.Exchange.Internals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mapna.Transmittals.Exchange.Services.Queues
{
    internal static class QueueContextExtensions
    {
        public static ITransmittalRepository GetRepository(this QueueContextBase context)
        {
            return context.ServiceProvider.GetService<ITransmittalRepository>();
        }
        public static void SendLog(this QueueContextBase context, LogLevel level, string message, params object[] args)
        {
            context.GetLogger().Log(level, message, args);
            if (level >= LogLevel.Information)
            {
                context.GetRepository().SendLog(level, message, args);
            }
        }

    }

}
