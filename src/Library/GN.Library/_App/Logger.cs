using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GN
{

	public interface ILogger_Deprecated
	{
		void Log(LogLevel level, string message, params object[] args);
	}
	public interface IAbstractLogger<T> : ILogger_Deprecated
	{


	}

	class LoggerFacrory
	{
		public static IAbstractLogger<T> CreateLogger<T>(ILoggerFactory factory)
		{
			return new AbstractLogger<T>(factory?.CreateLogger<T>());
		}

		public static ILogger_Deprecated CreateLogger(ILoggerFactory factory, Type type)
		{
			return new AbstractLogger(factory?.CreateLogger(type));
		}

	}
	class AbstractLogger : ILogger_Deprecated
	{
		private Microsoft.Extensions.Logging.ILogger logger;
		public AbstractLogger()
		{
			this.logger = null;
		}
		public AbstractLogger(Microsoft.Extensions.Logging.ILogger logger)
		{
			this.logger = logger;
		}
		public void Log(LogLevel level, string message, params object[] args)
		{

			logger?.Log(level, message, args);
		}
	}

	class AbstractLogger<T> : AbstractLogger, IAbstractLogger<T>
	{
		public AbstractLogger(Microsoft.Extensions.Logging.ILogger<T> logger) : base(logger)
		{
		}
	}


	public static partial class Extensions
	{
		public static void InfoFormat(this ILogger_Deprecated This, string fmt, params object[] args)
		{
			This.Log(LogLevel.Information, fmt, args);
		}
		public static void LogInformation(this ILogger_Deprecated This, string fmt, params object[] args)
		{
			This.Log(LogLevel.Information, fmt, args);
		}
		public static void DebugFormat(this ILogger_Deprecated This, string fmt, params object[] args)
		{
			This.Log(LogLevel.Debug, fmt, args);
		}
		public static void LogDebug(this ILogger_Deprecated This, string fmt, params object[] args)
		{
			This.Log(LogLevel.Debug, fmt, args);
		}
		public static void LogCritical(this ILogger_Deprecated This, string fmt, params object[] args)
		{
			This.Log(LogLevel.Critical, fmt, args);
		}
		public static void LogError(this ILogger_Deprecated This, string fmt, params object[] args)
		{
			This.Log(LogLevel.Error, fmt, args);
		}
		public static void ErrorFormat(this ILogger_Deprecated This, string fmt, params object[] args)
		{
			This.Log(LogLevel.Error, fmt, args);
		}
		public static void LogWarning(this ILogger_Deprecated This, string fmt, params object[] args)
		{
			This.Log(LogLevel.Error, fmt, args);
		}
		public static void WarningFormat(this ILogger_Deprecated This, string fmt, params object[] args)
		{
			This.Log(LogLevel.Error, fmt, args);
		}
		public static void TraceFormat(this ILogger_Deprecated This, string fmt, params object[] args)
		{
			This.Log(LogLevel.Trace, fmt, args);
		}
		public static void LogTrace(this ILogger_Deprecated This, string fmt, params object[] args)
		{
			This.Log(LogLevel.Trace, fmt, args);
		}
		public static bool HandleException(this ILogger_Deprecated This, Exception e)
		{
			return false;
		}
        public static void MethodStart(this ILogger_Deprecated This, string fmt=null, params object[] args)
        {

        }
        public static void MethodReturns(this ILogger_Deprecated This, bool success)
        {

        }

    }

}
