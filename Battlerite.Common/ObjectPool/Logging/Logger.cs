using System;

namespace CodeProject.ObjectPool.Logging
{
	public delegate bool Logger(LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters);
}
