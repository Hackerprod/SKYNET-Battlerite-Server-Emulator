using System;

namespace CodeProject.ObjectPool.Logging
{
	internal class LoggerExecutionWrapper : ILog
	{
		private readonly Logger _logger;

		private readonly Func<bool> _getIsDisabled;

		internal const string FailedToGenerateLogMessage = "Failed to generate log message";

		internal Logger WrappedLogger => _logger;

		internal LoggerExecutionWrapper(Logger logger, Func<bool> getIsDisabled = null)
		{
			_logger = logger;
			_getIsDisabled = (getIsDisabled ?? ((Func<bool>)(() => false)));
		}

		public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters)
		{
			if (_getIsDisabled())
			{
				return false;
			}
			if (messageFunc == null)
			{
				return _logger(logLevel, null, null);
			}
			Func<string> messageFunc2 = delegate
			{
				try
				{
					return messageFunc();
				}
				catch (Exception exception2)
				{
					Log(LogLevel.Error, () => "Failed to generate log message", exception2);
				}
				return null;
			};
			return _logger(logLevel, messageFunc2, exception, formatParameters);
		}
	}
}
