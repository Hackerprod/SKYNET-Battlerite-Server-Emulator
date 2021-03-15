using System;
using System.Linq.Expressions;
using System.Reflection;

namespace CodeProject.ObjectPool.Logging.LogProviders
{
	internal class SerilogLogProvider : LogProviderBase
	{
		internal class SerilogLogger
		{
			private readonly object _logger;

			private static readonly object DebugLevel;

			private static readonly object ErrorLevel;

			private static readonly object FatalLevel;

			private static readonly object InformationLevel;

			private static readonly object VerboseLevel;

			private static readonly object WarningLevel;

			private static readonly Func<object, object, bool> IsEnabled;

			private static readonly Action<object, object, string, object[]> Write;

			private static readonly Action<object, object, Exception, string, object[]> WriteException;

			static SerilogLogger()
			{
				Type type = Type.GetType("Serilog.Events.LogEventLevel, Serilog");
				if (type == null)
				{
					throw new InvalidOperationException("Type Serilog.Events.LogEventLevel was not found.");
				}
				DebugLevel = Enum.Parse(type, "Debug", ignoreCase: false);
				ErrorLevel = Enum.Parse(type, "Error", ignoreCase: false);
				FatalLevel = Enum.Parse(type, "Fatal", ignoreCase: false);
				InformationLevel = Enum.Parse(type, "Information", ignoreCase: false);
				VerboseLevel = Enum.Parse(type, "Verbose", ignoreCase: false);
				WarningLevel = Enum.Parse(type, "Warning", ignoreCase: false);
				Type type2 = Type.GetType("Serilog.ILogger, Serilog");
				if (type2 == null)
				{
					throw new InvalidOperationException("Type Serilog.ILogger was not found.");
				}
				MethodInfo methodPortable = type2.GetMethodPortable("IsEnabled", type);
				ParameterExpression parameterExpression = Expression.Parameter(typeof(object));
				UnaryExpression instance = Expression.Convert(parameterExpression, type2);
				ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object));
				UnaryExpression unaryExpression = Expression.Convert(parameterExpression2, type);
				IsEnabled = Expression.Lambda<Func<object, object, bool>>(Expression.Call(instance, methodPortable, unaryExpression), new ParameterExpression[2]
				{
					parameterExpression,
					parameterExpression2
				}).Compile();
				MethodInfo methodPortable2 = type2.GetMethodPortable("Write", type, typeof(string), typeof(object[]));
				ParameterExpression parameterExpression3 = Expression.Parameter(typeof(string));
				ParameterExpression parameterExpression4 = Expression.Parameter(typeof(object[]));
				Write = Expression.Lambda<Action<object, object, string, object[]>>(Expression.Call(instance, methodPortable2, unaryExpression, parameterExpression3, parameterExpression4), new ParameterExpression[4]
				{
					parameterExpression,
					parameterExpression2,
					parameterExpression3,
					parameterExpression4
				}).Compile();
				MethodInfo methodPortable3 = type2.GetMethodPortable("Write", type, typeof(Exception), typeof(string), typeof(object[]));
				ParameterExpression parameterExpression5 = Expression.Parameter(typeof(Exception));
				WriteException = Expression.Lambda<Action<object, object, Exception, string, object[]>>(Expression.Call(instance, methodPortable3, unaryExpression, parameterExpression5, parameterExpression3, parameterExpression4), new ParameterExpression[5]
				{
					parameterExpression,
					parameterExpression2,
					parameterExpression5,
					parameterExpression3,
					parameterExpression4
				}).Compile();
			}

			internal SerilogLogger(object logger)
			{
				_logger = logger;
			}

			public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
			{
				object obj = TranslateLevel(logLevel);
				if (messageFunc == null)
				{
					return IsEnabled(_logger, obj);
				}
				if (!IsEnabled(_logger, obj))
				{
					return false;
				}
				if (exception != null)
				{
					LogException(obj, messageFunc, exception, formatParameters);
				}
				else
				{
					LogMessage(obj, messageFunc, formatParameters);
				}
				return true;
			}

			private void LogMessage(object translatedLevel, Func<string> messageFunc, object[] formatParameters)
			{
				Write(_logger, translatedLevel, messageFunc(), formatParameters);
			}

			private void LogException(object logLevel, Func<string> messageFunc, Exception exception, object[] formatParams)
			{
				WriteException(_logger, logLevel, exception, messageFunc(), formatParams);
			}

			private static object TranslateLevel(LogLevel logLevel)
			{
				switch (logLevel)
				{
				case LogLevel.Fatal:
					return FatalLevel;
				case LogLevel.Error:
					return ErrorLevel;
				case LogLevel.Warn:
					return WarningLevel;
				case LogLevel.Info:
					return InformationLevel;
				case LogLevel.Trace:
					return VerboseLevel;
				default:
					return DebugLevel;
				}
			}
		}

		private readonly Func<string, object> _getLoggerByNameDelegate;

		private static bool s_providerIsAvailableOverride = true;

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

		public SerilogLogProvider()
		{
			if (!IsLoggerAvailable())
			{
				throw new InvalidOperationException("Serilog.Log not found");
			}
			_getLoggerByNameDelegate = GetForContextMethodCall();
		}

		public override Logger GetLogger(string name)
		{
			return new SerilogLogger(_getLoggerByNameDelegate(name)).Log;
		}

		internal static bool IsLoggerAvailable()
		{
			if (ProviderIsAvailableOverride)
			{
				return GetLogManagerType() != null;
			}
			return false;
		}

		protected override OpenNdc GetOpenNdcMethod()
		{
			return (string message) => GetPushProperty()("NDC", message);
		}

		protected override OpenMdc GetOpenMdcMethod()
		{
			return (string key, string value) => GetPushProperty()(key, value);
		}

		private static Func<string, string, IDisposable> GetPushProperty()
		{
			MethodInfo methodPortable = (Type.GetType("Serilog.Context.LogContext, Serilog") ?? Type.GetType("Serilog.Context.LogContext, Serilog.FullNetFx")).GetMethodPortable("PushProperty", typeof(string), typeof(object), typeof(bool));
			ParameterExpression parameterExpression = Expression.Parameter(typeof(string), "name");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object), "value");
			ParameterExpression parameterExpression3 = Expression.Parameter(typeof(bool), "destructureObjects");
			MethodCallExpression body = Expression.Call(null, methodPortable, parameterExpression, parameterExpression2, parameterExpression3);
			Func<string, object, bool, IDisposable> pushProperty = Expression.Lambda<Func<string, object, bool, IDisposable>>(body, new ParameterExpression[3]
			{
				parameterExpression,
				parameterExpression2,
				parameterExpression3
			}).Compile();
			return (string key, string value) => pushProperty(key, value, arg3: false);
		}

		private static Type GetLogManagerType()
		{
			return Type.GetType("Serilog.Log, Serilog");
		}

		private static Func<string, object> GetForContextMethodCall()
		{
			MethodInfo methodPortable = GetLogManagerType().GetMethodPortable("ForContext", typeof(string), typeof(object), typeof(bool));
			ParameterExpression parameterExpression = Expression.Parameter(typeof(string), "propertyName");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object), "value");
			ParameterExpression parameterExpression3 = Expression.Parameter(typeof(bool), "destructureObjects");
			MethodCallExpression body = Expression.Call(null, methodPortable, new Expression[3]
			{
				parameterExpression,
				parameterExpression2,
				parameterExpression3
			});
			Func<string, object, bool, object> func = Expression.Lambda<Func<string, object, bool, object>>(body, new ParameterExpression[3]
			{
				parameterExpression,
				parameterExpression2,
				parameterExpression3
			}).Compile();
			return (string name) => func("SourceContext", name, arg3: false);
		}
	}
}
