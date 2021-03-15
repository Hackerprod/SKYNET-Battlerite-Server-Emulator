using System;

namespace CodeProject.ObjectPool.Logging.LogProviders
{
	internal class LoupeLogProvider : LogProviderBase
	{
		internal delegate void WriteDelegate(int severity, string logSystem, int skipFrames, Exception exception, bool attributeToException, int writeMode, string detailsXml, string category, string caption, string description, params object[] args);

		internal class LoupeLogger
		{
			private const string LogSystem = "LibLog";

			private readonly string _category;

			private readonly WriteDelegate _logWriteDelegate;

			private readonly int _skipLevel;

			internal LoupeLogger(string category, WriteDelegate logWriteDelegate)
			{
				_category = category;
				_logWriteDelegate = logWriteDelegate;
				_skipLevel = 1;
			}

			public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
			{
				if (messageFunc == null)
				{
					return true;
				}
				messageFunc = LogMessageFormatter.SimulateStructuredLogging(messageFunc, formatParameters);
				_logWriteDelegate(ToLogMessageSeverity(logLevel), "LibLog", _skipLevel, exception, true, 0, null, _category, null, messageFunc());
				return true;
			}

			private static int ToLogMessageSeverity(LogLevel logLevel)
			{
				switch (logLevel)
				{
				case LogLevel.Trace:
					return TraceEventTypeValues.Verbose;
				case LogLevel.Debug:
					return TraceEventTypeValues.Verbose;
				case LogLevel.Info:
					return TraceEventTypeValues.Information;
				case LogLevel.Warn:
					return TraceEventTypeValues.Warning;
				case LogLevel.Error:
					return TraceEventTypeValues.Error;
				case LogLevel.Fatal:
					return TraceEventTypeValues.Critical;
				default:
					throw new ArgumentOutOfRangeException("logLevel");
				}
			}
		}

		private static bool s_providerIsAvailableOverride = true;

		private readonly WriteDelegate _logWriteDelegate;

		public static bool ProviderIsAvailableOverride
		{
			get
			{
				return s_providerIsAvailableOverride;
			}
			set
			{
				s_providerIsAvailableOverride = value;
			}
		}

		public LoupeLogProvider()
		{
			if (!IsLoggerAvailable())
			{
				throw new InvalidOperationException("Gibraltar.Agent.Log (Loupe) not found");
			}
			_logWriteDelegate = GetLogWriteDelegate();
		}

		public override Logger GetLogger(string name)
		{
			return new LoupeLogger(name, _logWriteDelegate).Log;
		}

		public static bool IsLoggerAvailable()
		{
			if (ProviderIsAvailableOverride)
			{
				return GetLogManagerType() != null;
			}
			return false;
		}

		private static Type GetLogManagerType()
		{
			return Type.GetType("Gibraltar.Agent.Log, Gibraltar.Agent");
		}

		private static WriteDelegate GetLogWriteDelegate()
		{
			Type logManagerType = GetLogManagerType();
			Type type = Type.GetType("Gibraltar.Agent.LogMessageSeverity, Gibraltar.Agent");
			Type type2 = Type.GetType("Gibraltar.Agent.LogWriteMode, Gibraltar.Agent");
			return (WriteDelegate)logManagerType.GetMethodPortable("Write", type, typeof(string), typeof(int), typeof(Exception), typeof(bool), type2, typeof(string), typeof(string), typeof(string), typeof(string), typeof(object[])).CreateDelegate(typeof(WriteDelegate));
		}
	}
}
