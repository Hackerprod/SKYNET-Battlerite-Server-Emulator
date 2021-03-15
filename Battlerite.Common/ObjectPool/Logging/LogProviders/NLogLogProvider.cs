using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CodeProject.ObjectPool.Logging.LogProviders
{
	internal class NLogLogProvider : LogProviderBase
	{
		internal class NLogLogger
		{
			private readonly dynamic _logger;

			private static readonly Func<string, object, string, Exception, object> _logEventInfoFact;

			private static readonly object _levelTrace;

			private static readonly object _levelDebug;

			private static readonly object _levelInfo;

			private static readonly object _levelWarn;

			private static readonly object _levelError;

			private static readonly object _levelFatal;

			static NLogLogger()
			{
				try
				{
					Type type = Type.GetType("NLog.LogLevel, NLog");
					if (type == null)
					{
						throw new InvalidOperationException("Type NLog.LogLevel was not found.");
					}
					List<FieldInfo> source = type.GetFieldsPortable().ToList();
					_levelTrace = source.First((FieldInfo x) => x.Name == "Trace").GetValue(null);
					_levelDebug = source.First((FieldInfo x) => x.Name == "Debug").GetValue(null);
					_levelInfo = source.First((FieldInfo x) => x.Name == "Info").GetValue(null);
					_levelWarn = source.First((FieldInfo x) => x.Name == "Warn").GetValue(null);
					_levelError = source.First((FieldInfo x) => x.Name == "Error").GetValue(null);
					_levelFatal = source.First((FieldInfo x) => x.Name == "Fatal").GetValue(null);
					Type type2 = Type.GetType("NLog.LogEventInfo, NLog");
					if (type2 == null)
					{
						throw new InvalidOperationException("Type NLog.LogEventInfo was not found.");
					}
					MethodInfo methodPortable = type2.GetMethodPortable("Create", type, typeof(string), typeof(Exception), typeof(IFormatProvider), typeof(string), typeof(object[]));
					ParameterExpression parameterExpression = Expression.Parameter(typeof(string));
					ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object));
					ParameterExpression parameterExpression3 = Expression.Parameter(typeof(string));
					ParameterExpression parameterExpression4 = Expression.Parameter(typeof(Exception));
					UnaryExpression unaryExpression = Expression.Convert(parameterExpression2, type);
					_logEventInfoFact = Expression.Lambda<Func<string, object, string, Exception, object>>(Expression.Call(null, methodPortable, unaryExpression, parameterExpression, parameterExpression4, Expression.Constant(null, typeof(IFormatProvider)), parameterExpression3, Expression.Constant(null, typeof(object[]))), new ParameterExpression[4]
					{
						parameterExpression,
						parameterExpression2,
						parameterExpression3,
						parameterExpression4
					}).Compile();
				}
				catch
				{
				}
			}

			internal NLogLogger(dynamic logger)
			{
				_logger = logger;
			}

			public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
			{
				if (messageFunc == null)
				{
					return IsLogLevelEnable(logLevel);
				}
				messageFunc = LogMessageFormatter.SimulateStructuredLogging(messageFunc, formatParameters);
				if (_logEventInfoFact != null)
				{
					if (IsLogLevelEnable(logLevel))
					{
						object obj = TranslateLevel(logLevel);
						StackTrace stackTrace = new StackTrace();
						Type type = GetType();
						Type typeFromHandle = typeof(LoggerExecutionWrapper);
						Type typeFromHandle2 = typeof(LogExtensions);
						Type type2 = null;
						for (int i = 0; i < stackTrace.FrameCount; i++)
						{
							Type declaringType = stackTrace.GetFrame(i).GetMethod().DeclaringType;
							if (!IsInTypeHierarchy(type, declaringType) && !IsInTypeHierarchy(typeFromHandle, declaringType) && !IsInTypeHierarchy(typeFromHandle2, declaringType))
							{
								if (i > 1)
								{
									type2 = stackTrace.GetFrame(i - 1).GetMethod().DeclaringType;
								}
								break;
							}
						}
						if (!(type2 != null))
						{
							_logger.Log(_logEventInfoFact(_logger.Name, obj, messageFunc(), exception));
						}
						else
						{
							_logger.Log(type2, _logEventInfoFact(_logger.Name, obj, messageFunc(), exception));
						}
						return true;
					}
					return false;
				}
				if (exception != null)
				{
					return LogException(logLevel, messageFunc, exception);
				}
				switch (logLevel)
				{
				case LogLevel.Debug:
					if (_logger.IsDebugEnabled)
					{
						_logger.Debug(messageFunc());
						return true;
					}
					break;
				case LogLevel.Info:
					if (_logger.IsInfoEnabled)
					{
						_logger.Info(messageFunc());
						return true;
					}
					break;
				case LogLevel.Warn:
					if (_logger.IsWarnEnabled)
					{
						_logger.Warn(messageFunc());
						return true;
					}
					break;
				case LogLevel.Error:
					if (_logger.IsErrorEnabled)
					{
						_logger.Error(messageFunc());
						return true;
					}
					break;
				case LogLevel.Fatal:
					if (_logger.IsFatalEnabled)
					{
						_logger.Fatal(messageFunc());
						return true;
					}
					break;
				default:
					if (_logger.IsTraceEnabled)
					{
						_logger.Trace(messageFunc());
						return true;
					}
					break;
				}
				return false;
			}

			private static bool IsInTypeHierarchy(Type currentType, Type checkType)
			{
				while (currentType != null && currentType != typeof(object))
				{
					if (currentType == checkType)
					{
						return true;
					}
					currentType = currentType.GetBaseTypePortable();
				}
				return false;
			}

			private bool LogException(LogLevel logLevel, Func<string> messageFunc, Exception exception)
			{
				switch (logLevel)
				{
				case LogLevel.Debug:
					if (_logger.IsDebugEnabled)
					{
						_logger.DebugException(messageFunc(), exception);
						return true;
					}
					break;
				case LogLevel.Info:
					if (_logger.IsInfoEnabled)
					{
						_logger.InfoException(messageFunc(), exception);
						return true;
					}
					break;
				case LogLevel.Warn:
					if (_logger.IsWarnEnabled)
					{
						_logger.WarnException(messageFunc(), exception);
						return true;
					}
					break;
				case LogLevel.Error:
					if (_logger.IsErrorEnabled)
					{
						_logger.ErrorException(messageFunc(), exception);
						return true;
					}
					break;
				case LogLevel.Fatal:
					if (_logger.IsFatalEnabled)
					{
						_logger.FatalException(messageFunc(), exception);
						return true;
					}
					break;
				default:
					if (_logger.IsTraceEnabled)
					{
						_logger.TraceException(messageFunc(), exception);
						return true;
					}
					break;
				}
				return false;
			}

			private bool IsLogLevelEnable(LogLevel logLevel)
			{
				switch (logLevel)
				{
				case LogLevel.Debug:
					return (byte)_logger.IsDebugEnabled != 0;
				case LogLevel.Info:
					return (byte)_logger.IsInfoEnabled != 0;
				case LogLevel.Warn:
					return (byte)_logger.IsWarnEnabled != 0;
				case LogLevel.Error:
					return (byte)_logger.IsErrorEnabled != 0;
				case LogLevel.Fatal:
					return (byte)_logger.IsFatalEnabled != 0;
				default:
					return (byte)_logger.IsTraceEnabled != 0;
				}
			}

			private object TranslateLevel(LogLevel logLevel)
			{
				switch (logLevel)
				{
				case LogLevel.Trace:
					return _levelTrace;
				case LogLevel.Debug:
					return _levelDebug;
				case LogLevel.Info:
					return _levelInfo;
				case LogLevel.Warn:
					return _levelWarn;
				case LogLevel.Error:
					return _levelError;
				case LogLevel.Fatal:
					return _levelFatal;
				default:
					throw new ArgumentOutOfRangeException("logLevel", logLevel, null);
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

		public NLogLogProvider()
		{
			if (!IsLoggerAvailable())
			{
				throw new InvalidOperationException("NLog.LogManager not found");
			}
			_getLoggerByNameDelegate = GetGetLoggerMethodCall();
		}

		public override Logger GetLogger(string name)
		{
			return new NLogLogger(_getLoggerByNameDelegate(name)).Log;
		}

		public static bool IsLoggerAvailable()
		{
			if (ProviderIsAvailableOverride)
			{
				return GetLogManagerType() != null;
			}
			return false;
		}

		protected override OpenNdc GetOpenNdcMethod()
		{
			MethodInfo methodPortable = TypeExtensions.GetMethodPortable(Type.GetType("NLog.NestedDiagnosticsContext, NLog"), "Push", typeof(string));
			ParameterExpression parameterExpression = Expression.Parameter(typeof(string), "message");
			return Expression.Lambda<OpenNdc>(Expression.Call(null, methodPortable, parameterExpression), new ParameterExpression[1]
			{
				parameterExpression
			}).Compile();
		}

		protected override OpenMdc GetOpenMdcMethod()
		{
			Type type = Type.GetType("NLog.MappedDiagnosticsContext, NLog");
			MethodInfo methodPortable = type.GetMethodPortable("Set", typeof(string), typeof(string));
			MethodInfo methodPortable2 = TypeExtensions.GetMethodPortable(type, "Remove", typeof(string));
			ParameterExpression parameterExpression = Expression.Parameter(typeof(string), "key");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(string), "value");
			MethodCallExpression body = Expression.Call(null, methodPortable, parameterExpression, parameterExpression2);
			MethodCallExpression body2 = Expression.Call(null, methodPortable2, parameterExpression);
			Action<string, string> set = Expression.Lambda<Action<string, string>>(body, new ParameterExpression[2]
			{
				parameterExpression,
				parameterExpression2
			}).Compile();
			Action<string> remove = Expression.Lambda<Action<string>>(body2, new ParameterExpression[1]
			{
				parameterExpression
			}).Compile();
			return delegate(string key, string value)
			{
				set(key, value);
				return new DisposableAction(delegate
				{
					remove(key);
				});
			};
		}

		private static Type GetLogManagerType()
		{
			return Type.GetType("NLog.LogManager, NLog");
		}

		private static Func<string, object> GetGetLoggerMethodCall()
		{
			MethodInfo methodPortable = TypeExtensions.GetMethodPortable(GetLogManagerType(), "GetLogger", typeof(string));
			ParameterExpression parameterExpression = Expression.Parameter(typeof(string), "name");
			return Expression.Lambda<Func<string, object>>(Expression.Call(null, methodPortable, parameterExpression), new ParameterExpression[1]
			{
				parameterExpression
			}).Compile();
		}
	}
}
