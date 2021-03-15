using System;

namespace CodeProject.ObjectPool.Logging
{
	internal interface ILog
	{
		bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters);
	}
}