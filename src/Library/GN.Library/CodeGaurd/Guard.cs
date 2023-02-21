using GN.CodeGuard.Internals;
using System;

namespace GN.CodeGuard
{
	public static class Guard
	{
		public static ArgBase<T> That<T>(T argument, string argumentName = "")
		{
			return new ArgBase<T>(argument, argumentName);
		}

		public static void Requires(bool condition, string fmt = null, params object[] args)
		{
			if (!condition)
				throw new ArgumentException(string.IsNullOrEmpty(fmt) ? "Invalid Argument" : string.Format(fmt, args));
		}
		public static void Ensures(bool condition, string fmt = null, params object[] args)
		{
			if (!condition)
				throw new InvalidProgramException(string.IsNullOrEmpty(fmt) ? "Invalid Operation" : string.Format(fmt, args));
		}

	}
}