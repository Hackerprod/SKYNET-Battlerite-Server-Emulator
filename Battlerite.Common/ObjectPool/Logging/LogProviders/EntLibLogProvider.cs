using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace CodeProject.ObjectPool.Logging.LogProviders
{
	internal class EntLibLogProvider : LogProviderBase
	{
		internal class EntLibLogger
		{
			private readonly string _loggerName;

			private readonly Action<string, string, int> _writeLog;

			private readonly Func<string, int, bool> _shouldLog;

			internal EntLibLogger(string loggerName, Action<string, string, int> writeLog, Func<string, int, bool> shouldLog)
			{
				_loggerName = loggerName;
				_writeLog = writeLog;
				_shouldLog = shouldLog;
			}

			public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
			{
				int num = MapSeverity(logLevel);
				if (messageFunc == null)
				{
					return _shouldLog(_loggerName, num);
				}
				messageFunc = LogMessageFormatter.SimulateStructuredLogging(messageFunc, formatParameters);
				if (exception != null)
				{
					return LogException(logLevel, messageFunc, exception);
				}
				_writeLog(_loggerName, messageFunc(), num);
				return true;
			}

			public bool LogException(LogLevel logLevel, Func<string> messageFunc, Exception exception)
			{
				int arg = MapSeverity(logLevel);
				string arg2 = messageFunc() + Environment.NewLine + exception;
				_writeLog(_loggerName, arg2, arg);
				return true;
			}

			private static int MapSeverity(LogLevel logLevel)
			{
				switch (logLevel)
				{
				case LogLevel.Fatal:
					return TraceEventTypeValues.Critical;
				case LogLevel.Error:
					return TraceEventTypeValues.Error;
				case LogLevel.Warn:
					return TraceEventTypeValues.Warning;
				case LogLevel.Info:
					return TraceEventTypeValues.Information;
				default:
					return TraceEventTypeValues.Verbose;
				}
			}
		}

		private const string TypeTemplate = "Microsoft.Practices.EnterpriseLibrary.Logging.{0}, Microsoft.Practices.EnterpriseLibrary.Logging";

		private static bool s_providerIsAvailableOverride;

		private static readonly Type LogEntryType;

		private static readonly Type LoggerType;

		private static readonly Type TraceEventTypeType;

		private static readonly Action<string, string, int> WriteLogEntry;

		private static readonly Func<string, int, bool> ShouldLogEntry;

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

		static EntLibLogProvider()
		{
			s_providerIsAvailableOverride = true;
			LogEntryType = Type.GetType(string.Format(CultureInfo.InvariantCulture, "Microsoft.Practices.EnterpriseLibrary.Logging.{0}, Microsoft.Practices.EnterpriseLibrary.Logging", new object[1]
			{
				"LogEntry"
			}));
			LoggerType = Type.GetType(string.Format(CultureInfo.InvariantCulture, "Microsoft.Practices.EnterpriseLibrary.Logging.{0}, Microsoft.Practices.EnterpriseLibrary.Logging", new object[1]
			{
				"Logger"
			}));
			TraceEventTypeType = TraceEventTypeValues.Type;
			if (!(LogEntryType == null) && !(TraceEventTypeType == null) && !(LoggerType == null))
			{
				WriteLogEntry = GetWriteLogEntry();
				ShouldLogEntry = GetShouldLogEntry();
			}
		}

		public EntLibLogProvider()
		{
			if (!IsLoggerAvailable())
			{
				throw new InvalidOperationException("Microsoft.Practices.EnterpriseLibrary.Logging.Logger not found");
			}
		}

		public override Logger GetLogger(string name)
		{
			return new EntLibLogger(name, WriteLogEntry, ShouldLogEntry).Log;
		}

		internal static bool IsLoggerAvailable()
		{
			if (ProviderIsAvailableOverride && TraceEventTypeType != null)
			{
				return LogEntryType != null;
			}
			return false;
		}

		private static Action<string, string, int> GetWriteLogEntry()
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(string), "logName");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(string), "message");
			ParameterExpression parameterExpression3 = Expression.Parameter(typeof(int), "severity");
			MemberInitExpression writeLogExpression = GetWriteLogExpression(parameterExpression2, Expression.Convert(parameterExpression3, TraceEventTypeType), parameterExpression);
			return Expression.Lambda<Action<string, string, int>>(Expression.Call(LoggerType.GetMethodPortable("Write", LogEntryType), writeLogExpression), new ParameterExpression[3]
			{
				parameterExpression,
				parameterExpression2,
				parameterExpression3
			}).Compile();
		}

		private static Func<string, int, bool> GetShouldLogEntry()
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(string), "logName");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(int), "severity");
			MemberInitExpression writeLogExpression = GetWriteLogExpression(Expression.Constant("***dummy***"), Expression.Convert(parameterExpression2, TraceEventTypeType), parameterExpression);
			return Expression.Lambda<Func<string, int, bool>>(Expression.Call(LoggerType.GetMethodPortable("ShouldLog", LogEntryType), writeLogExpression), new ParameterExpression[2]
			{
				parameterExpression,
				parameterExpression2
			}).Compile();
		}

		private static MemberInitExpression GetWriteLogExpression(Expression message, Expression severityParameter, ParameterExpression logNameParameter)
		{
			Type logEntryType = LogEntryType;
			return Expression.MemberInit(Expression.New(logEntryType), Expression.Bind(logEntryType.GetPropertyPortable("Message"), message), Expression.Bind(logEntryType.GetPropertyPortable("Severity"), severityParameter), Expression.Bind(logEntryType.GetPropertyPortable("TimeStamp"), Expression.Property(null, typeof(DateTime).GetPropertyPortable("UtcNow"))), Expression.Bind(logEntryType.GetPropertyPortable("Categories"), Expression.ListInit(Expression.New(typeof(List<string>)), TypeExtensions.GetMethodPortable(typeof(List<string>), "Add", typeof(string)), logNameParameter)));
		}
	}
}
