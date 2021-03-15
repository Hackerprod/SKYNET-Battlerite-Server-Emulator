using CodeProject.ObjectPool.Logging.LogProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CodeProject.ObjectPool.Logging
{
	public static class LogProvider
	{
		internal delegate bool IsLoggerAvailable();

		internal delegate ILogProvider CreateLogProvider();

		internal class NoOpLogger : ILog
		{
			internal static readonly NoOpLogger Instance = new NoOpLogger();

			public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
			{
				return false;
			}
		}

		private const string NullLogProvider = "Current Log Provider is not set. Call SetCurrentLogProvider with a non-null value first.";

		private static dynamic s_currentLogProvider;

		private static Action<ILogProvider> s_onCurrentLogProviderSet;

		internal static readonly List<Tuple<IsLoggerAvailable, CreateLogProvider>> LogProviderResolvers;

		public static bool IsDisabled
		{
			get;
			set;
		}

		internal static Action<ILogProvider> OnCurrentLogProviderSet
		{
			set
			{
				s_onCurrentLogProviderSet = value;
				RaiseOnCurrentLogProviderSet();
			}
		}

		internal static ILogProvider CurrentLogProvider => (ILogProvider)s_currentLogProvider;

		static LogProvider()
		{
			LogProviderResolvers = new List<Tuple<IsLoggerAvailable, CreateLogProvider>>
			{
				new Tuple<IsLoggerAvailable, CreateLogProvider>(SerilogLogProvider.IsLoggerAvailable, () => new SerilogLogProvider()),
				new Tuple<IsLoggerAvailable, CreateLogProvider>(NLogLogProvider.IsLoggerAvailable, () => new NLogLogProvider()),
				new Tuple<IsLoggerAvailable, CreateLogProvider>(Log4NetLogProvider.IsLoggerAvailable, () => new Log4NetLogProvider()),
				new Tuple<IsLoggerAvailable, CreateLogProvider>(EntLibLogProvider.IsLoggerAvailable, () => new EntLibLogProvider()),
				new Tuple<IsLoggerAvailable, CreateLogProvider>(LoupeLogProvider.IsLoggerAvailable, () => new LoupeLogProvider())
			};
			IsDisabled = false;
		}

		public static void SetCurrentLogProvider(ILogProvider logProvider)
		{
			s_currentLogProvider = logProvider;
			RaiseOnCurrentLogProviderSet();
		}

		internal static ILog For<T>()
		{
			return GetLogger(typeof(T));
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		internal static ILog GetCurrentClassLogger()
		{
			return GetLogger(new StackFrame(1, fNeedFileInfo: false).GetMethod().DeclaringType);
		}

		internal static ILog GetLogger(Type type, string fallbackTypeName = "System.Object")
		{
			return GetLogger((type != null) ? type.FullName : fallbackTypeName);
		}

		internal static ILog GetLogger(string name)
		{
			ILogProvider logProvider = CurrentLogProvider ?? ResolveLogProvider();
			if (logProvider != null)
			{
				return new LoggerExecutionWrapper(logProvider.GetLogger(name), () => IsDisabled);
			}
			return NoOpLogger.Instance;
		}

		internal static IDisposable OpenNestedContext(string message)
		{
			ILogProvider logProvider = CurrentLogProvider ?? ResolveLogProvider();
			if (logProvider != null)
			{
				return logProvider.OpenNestedContext(message);
			}
			return new DisposableAction(delegate
			{
			});
		}

		internal static IDisposable OpenMappedContext(string key, string value)
		{
			ILogProvider logProvider = CurrentLogProvider ?? ResolveLogProvider();
			if (logProvider != null)
			{
				return logProvider.OpenMappedContext(key, value);
			}
			return new DisposableAction(delegate
			{
			});
		}

		private static void RaiseOnCurrentLogProviderSet()
		{
			if (s_onCurrentLogProviderSet != null)
			{
				s_onCurrentLogProviderSet(s_currentLogProvider);
			}
		}

		internal static ILogProvider ResolveLogProvider()
		{
			try
			{
				foreach (Tuple<IsLoggerAvailable, CreateLogProvider> logProviderResolver in LogProviderResolvers)
				{
					if (logProviderResolver.Item1())
					{
						return logProviderResolver.Item2();
					}
				}
			}
			catch (Exception arg)
			{
				Console.WriteLine("Exception occurred resolving a log provider. Logging for this assembly {0} is disabled. {1}", typeof(LogProvider).GetAssemblyPortable().FullName, arg);
			}
			return null;
		}
	}
}
