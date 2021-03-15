using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CodeProject.ObjectPool.Logging.LogProviders
{
	internal class Log4NetLogProvider : LogProviderBase
	{
		internal class Log4NetLogger
		{
			private readonly dynamic _logger;

			private static Type s_callerStackBoundaryType;

			private static readonly object CallerStackBoundaryTypeSync = new object();

			private readonly object _levelDebug;

			private readonly object _levelInfo;

			private readonly object _levelWarn;

			private readonly object _levelError;

			private readonly object _levelFatal;

			private readonly Func<object, object, bool> _isEnabledForDelegate;

			private readonly Action<object, object> _logDelegate;

			private readonly Func<object, Type, object, string, Exception, object> _createLoggingEvent;

			private readonly Action<object, string, object> _loggingEventPropertySetter;

			internal Log4NetLogger(dynamic logger)
			{
				_logger = logger.Logger;
				Type type = Type.GetType("log4net.Core.Level, log4net");
				if (type == null)
				{
					throw new InvalidOperationException("Type log4net.Core.Level was not found.");
				}
				List<FieldInfo> source = type.GetFieldsPortable().ToList();
				_levelDebug = source.First((FieldInfo x) => x.Name == "Debug").GetValue(null);
				_levelInfo = source.First((FieldInfo x) => x.Name == "Info").GetValue(null);
				_levelWarn = source.First((FieldInfo x) => x.Name == "Warn").GetValue(null);
				_levelError = source.First((FieldInfo x) => x.Name == "Error").GetValue(null);
				_levelFatal = source.First((FieldInfo x) => x.Name == "Fatal").GetValue(null);
				Type type2 = Type.GetType("log4net.Core.ILogger, log4net");
				if (type2 == null)
				{
					throw new InvalidOperationException("Type log4net.Core.ILogger, was not found.");
				}
				ParameterExpression parameterExpression = Expression.Parameter(typeof(object));
				UnaryExpression instanceCast = Expression.Convert(parameterExpression, type2);
				ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object));
				UnaryExpression levelCast = Expression.Convert(parameterExpression2, type);
				_isEnabledForDelegate = GetIsEnabledFor(type2, type, instanceCast, levelCast, parameterExpression, parameterExpression2);
				Type type3 = Type.GetType("log4net.Core.LoggingEvent, log4net");
				_createLoggingEvent = GetCreateLoggingEvent(parameterExpression, instanceCast, parameterExpression2, levelCast, type3);
				_logDelegate = GetLogDelegate(type2, type3, instanceCast, parameterExpression);
				_loggingEventPropertySetter = GetLoggingEventPropertySetter(type3);
			}

			private static Action<object, object> GetLogDelegate(Type loggerType, Type loggingEventType, UnaryExpression instanceCast, ParameterExpression instanceParam)
			{
				MethodInfo methodPortable = loggerType.GetMethodPortable("Log", loggingEventType);
				ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "loggingEvent");
				UnaryExpression unaryExpression = Expression.Convert(parameterExpression, loggingEventType);
				return Expression.Lambda<Action<object, object>>(Expression.Call(instanceCast, methodPortable, unaryExpression), new ParameterExpression[2]
				{
					instanceParam,
					parameterExpression
				}).Compile();
			}

			private static Func<object, Type, object, string, Exception, object> GetCreateLoggingEvent(ParameterExpression instanceParam, UnaryExpression instanceCast, ParameterExpression levelParam, UnaryExpression levelCast, Type loggingEventType)
			{
				ParameterExpression parameterExpression = Expression.Parameter(typeof(Type));
				ParameterExpression parameterExpression2 = Expression.Parameter(typeof(string));
				ParameterExpression parameterExpression3 = Expression.Parameter(typeof(Exception));
				PropertyInfo propertyPortable = loggingEventType.GetPropertyPortable("Repository");
				PropertyInfo propertyPortable2 = loggingEventType.GetPropertyPortable("Level");
				return Expression.Lambda<Func<object, Type, object, string, Exception, object>>(Expression.New(loggingEventType.GetConstructorPortable(typeof(Type), propertyPortable.PropertyType, typeof(string), propertyPortable2.PropertyType, typeof(object), typeof(Exception)), parameterExpression, Expression.Property(instanceCast, "Repository"), Expression.Property(instanceCast, "Name"), levelCast, parameterExpression2, parameterExpression3), new ParameterExpression[5]
				{
					instanceParam,
					parameterExpression,
					levelParam,
					parameterExpression2,
					parameterExpression3
				}).Compile();
			}

			private static Func<object, object, bool> GetIsEnabledFor(Type loggerType, Type logEventLevelType, UnaryExpression instanceCast, UnaryExpression levelCast, ParameterExpression instanceParam, ParameterExpression levelParam)
			{
				MethodInfo methodPortable = loggerType.GetMethodPortable("IsEnabledFor", logEventLevelType);
				return Expression.Lambda<Func<object, object, bool>>(Expression.Call(instanceCast, methodPortable, levelCast), new ParameterExpression[2]
				{
					instanceParam,
					levelParam
				}).Compile();
			}

			private static Action<object, string, object> GetLoggingEventPropertySetter(Type loggingEventType)
			{
				ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "loggingEvent");
				ParameterExpression parameterExpression2 = Expression.Parameter(typeof(string), "key");
				ParameterExpression parameterExpression3 = Expression.Parameter(typeof(object), "value");
				PropertyInfo propertyPortable = loggingEventType.GetPropertyPortable("Properties");
				PropertyInfo propertyPortable2 = propertyPortable.PropertyType.GetPropertyPortable("Item");
				return Expression.Lambda<Action<object, string, object>>(Expression.Assign(Expression.Property(Expression.Property(Expression.Convert(parameterExpression, loggingEventType), propertyPortable), propertyPortable2, parameterExpression2), parameterExpression3), new ParameterExpression[3]
				{
					parameterExpression,
					parameterExpression2,
					parameterExpression3
				}).Compile();
			}

			public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
			{
				if (messageFunc == null)
				{
					return IsLogLevelEnable(logLevel);
				}
				if (!IsLogLevelEnable(logLevel))
				{
					return false;
				}
				IEnumerable<string> patternMatches;
				string text = LogMessageFormatter.FormatStructuredMessage(messageFunc(), formatParameters, out patternMatches);
				if (s_callerStackBoundaryType == null)
				{
					lock (CallerStackBoundaryTypeSync)
					{
						StackTrace stackTrace = new StackTrace();
						Type type = GetType();
						s_callerStackBoundaryType = Type.GetType("LoggerExecutionWrapper");
						for (int i = 1; i < stackTrace.FrameCount; i++)
						{
							if (!IsInTypeHierarchy(type, stackTrace.GetFrame(i).GetMethod().DeclaringType))
							{
								s_callerStackBoundaryType = stackTrace.GetFrame(i - 1).GetMethod().DeclaringType;
								break;
							}
						}
					}
				}
				object obj = TranslateLevel(logLevel);
				object obj2 = _createLoggingEvent(_logger, s_callerStackBoundaryType, obj, text, exception);
				PopulateProperties(obj2, patternMatches, formatParameters);
				_logDelegate(_logger, obj2);
				return true;
			}

			private void PopulateProperties(object loggingEvent, IEnumerable<string> patternMatches, object[] formatParameters)
			{
				foreach (KeyValuePair<string, object> item in patternMatches.Zip(formatParameters, (string key, object value) => new KeyValuePair<string, object>(key, value)))
				{
					_loggingEventPropertySetter(loggingEvent, item.Key, item.Value);
				}
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

			private bool IsLogLevelEnable(LogLevel logLevel)
			{
				object obj = TranslateLevel(logLevel);
				return (byte)_isEnabledForDelegate(_logger, obj) != 0;
			}

			private object TranslateLevel(LogLevel logLevel)
			{
				switch (logLevel)
				{
				case LogLevel.Trace:
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

		public Log4NetLogProvider()
		{
			if (!IsLoggerAvailable())
			{
				throw new InvalidOperationException("log4net.LogManager not found");
			}
			_getLoggerByNameDelegate = GetGetLoggerMethodCall();
		}

		public override Logger GetLogger(string name)
		{
			return new Log4NetLogger(_getLoggerByNameDelegate(name)).Log;
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
			PropertyInfo propertyPortable = Type.GetType("log4net.LogicalThreadContext, log4net").GetPropertyPortable("Stacks");
			PropertyInfo propertyPortable2 = propertyPortable.PropertyType.GetPropertyPortable("Item");
			MethodInfo methodPortable = propertyPortable2.PropertyType.GetMethodPortable("Push");
			ParameterExpression parameterExpression = Expression.Parameter(typeof(string), "message");
			return Expression.Lambda<OpenNdc>(Expression.Call(Expression.Property(Expression.Property(null, propertyPortable), propertyPortable2, Expression.Constant("NDC")), methodPortable, parameterExpression), new ParameterExpression[1]
			{
				parameterExpression
			}).Compile();
		}

		protected override OpenMdc GetOpenMdcMethod()
		{
			PropertyInfo propertyPortable = Type.GetType("log4net.LogicalThreadContext, log4net").GetPropertyPortable("Properties");
			Type propertyType = propertyPortable.PropertyType;
			PropertyInfo propertyPortable2 = propertyType.GetPropertyPortable("Item");
			MethodInfo methodPortable = propertyType.GetMethodPortable("Remove");
			ParameterExpression parameterExpression = Expression.Parameter(typeof(string), "key");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(string), "value");
			MemberExpression instance = Expression.Property(null, propertyPortable);
			BinaryExpression body = Expression.Assign(Expression.Property(instance, propertyPortable2, parameterExpression), parameterExpression2);
			MethodCallExpression body2 = Expression.Call(instance, methodPortable, parameterExpression);
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
			return Type.GetType("log4net.LogManager, log4net");
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
